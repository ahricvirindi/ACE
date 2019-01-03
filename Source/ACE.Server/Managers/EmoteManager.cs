using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using ACE.Database.Models.Shard;
using ACE.DatLoader;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Entity.Actions;
using ACE.Server.Factories;
using ACE.Server.Network.Enum;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.WorldObjects;

using log4net;

using Position = ACE.Entity.Position;

namespace ACE.Server.Managers
{
    public partial class EmoteManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public WorldObject WorldObject;

        public DateTime NextAvailable = DateTime.UtcNow;

        /// <summary>
        /// Returns TRUE if this WorldObject is currently busy processing other emotes
        /// </summary>
        public bool IsBusy { get => DateTime.UtcNow < NextAvailable; }

        public bool Debug = false;

        public EmoteManager(WorldObject worldObject)
        {
            WorldObject = worldObject;
        }

        /// <summary>
        /// Executes an emote
        /// </summary>
        /// <param name="emoteSet">The parent set of this emote</param>
        /// <param name="emote">The emote to execute</param>
        /// <param name="targetObject">A target object, usually player</param>
        /// <param name="actionChain">Only used for passing to further sets</param>
        public void ExecuteEmote(BiotaPropertiesEmote emoteSet, BiotaPropertiesEmoteAction emote, WorldObject targetObject = null, ActionChain actionChain = null)
        {
            var player = targetObject as Player;
            var creature = WorldObject as Creature;
            var targetCreature = targetObject as Creature;

            var emoteType = (EmoteType)emote.Type;

            if (Debug)
                Console.WriteLine($"{WorldObject.Name}.ExecuteEmote({emoteType})");

            var text = emote.Message;

            switch ((EmoteType)emote.Type)
            {
                case EmoteType.Act:
                    // short for 'acting' text
                    var message = Replace(text, WorldObject, targetObject);
                    WorldObject.EnqueueBroadcast(new GameMessageSystemChat(message, ChatMessageType.Broadcast), 30.0f);
                    break;

                case EmoteType.Activate:

                    if (WorldObject.ActivationTarget > 0)
                    {
                        var activationTarget = WorldObject.CurrentLandblock?.GetObject(WorldObject.ActivationTarget);
                        activationTarget?.ActOnUse(WorldObject);
                    }
                    break;

                case EmoteType.AddCharacterTitle:

                    // emoteAction.Stat == null for all EmoteType.AddCharacterTitle entries in current db?
                    if (player != null && emote.Amount != 0)
                        player.AddTitle((CharacterTitle)emote.Amount);
                    break;

                case EmoteType.AddContract:

                    //if (player != null)
                        //Contracts werent in emote table
                        //player.AddContract(emote.Stat);
                    break;

                case EmoteType.AdminSpam:

                    var players = PlayerManager.GetAllOnline();
                    foreach (var onlinePlayer in players)
                        onlinePlayer.Session.Network.EnqueueSend(new GameMessageSystemChat(text, ChatMessageType.AdminTell));
                    break;

                case EmoteType.AwardLevelProportionalSkillXP:

                    if (player != null)
                        player.GrantLevelProportionalSkillXP((Skill)emote.Stat, emote.Percent ?? 0, (ulong)emote.Max);
                    break;

                case EmoteType.AwardLevelProportionalXP:

                    if (player != null)
                        player.GrantLevelProportionalXp(emote.Percent ?? 0, (ulong)emote.Max);
                    break;

                case EmoteType.AwardLuminance:

                    if (player != null)
                        player.GrantLuminance((long)emote.Amount);
                    break;

                case EmoteType.AwardNoShareXP:

                    if (player != null)
                    {
                        player.EarnXP((long)emote.Amount64);
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat("You've earned " + emote.Amount64.Value.ToString("N0") + " experience.", ChatMessageType.Broadcast));
                    }
                    break;

                case EmoteType.AwardSkillPoints:

                    if (player != null)
                        player.AwardSkillPoints((Skill)emote.Stat, (uint)emote.Amount, true);
                    break;

                case EmoteType.AwardSkillXP:

                    if (player != null)
                        player.RaiseSkillGameAction((Skill)emote.Stat, (uint)emote.Amount, true);
                    break;

                case EmoteType.AwardTrainingCredits:

                    if (player != null)
                        player.AddSkillCredits((int)emote.Amount, false);
                    break;

                case EmoteType.AwardXP:

                    if (player != null)
                    {
                        player.EarnXP((long)emote.Amount64);
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat("You've earned " + emote.Amount64.Value.ToString("N0") + " experience.", ChatMessageType.Broadcast));
                    }
                    break;

                case EmoteType.BLog:
                    // only one test drudge used this emoteAction.
                    break;

                case EmoteType.CastSpell:

                    // todo: missing windup?
                    if (creature != null && targetObject != null)
                        creature.CreateCreatureSpell(targetObject.Guid, (uint)emote.SpellId);
                    break;

                case EmoteType.CastSpellInstant:

                    if (creature != null)
                    {
                        var spell = new Spell((uint)emote.SpellId);
                        if (targetObject != null && spell.TargetEffect > 0)
                            creature.CreateCreatureSpell(targetObject.Guid, (uint)emote.SpellId);
                        else
                        {
                            creature.CreateCreatureSpell((uint)emote.SpellId);
                            creature.WarMagic(spell);   // only war magic?
                        }
                    }
                    break;

                case EmoteType.CloseMe:

                    if (targetObject != null)
                        targetObject.Close(WorldObject);
                    break;

                case EmoteType.CreateTreasure:
                    break;

                /* decrements a PropertyInt stat by some amount */
                case EmoteType.DecrementIntStat:

                    // only used by 1 emote in 16PY - check for lower bounds?
                    if (targetObject != null && emote.Stat != null)
                    {
                        var intProperty = (PropertyInt)emote.Stat;
                        var current = targetObject.GetProperty(intProperty) ?? 0;
                        current -= emote.Amount ?? 0;
                        targetObject.SetProperty(intProperty, current);

                        if (player != null)
                            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, intProperty, current));
                    }
                    break;

                case EmoteType.DecrementMyQuest:
                    break;

                case EmoteType.DecrementQuest:
                    // Used as part of the test drudge for events
                    break;

                case EmoteType.DeleteSelf:

                    WorldObject.Destroy();
                    break;

                case EmoteType.DirectBroadcast:

                    text = Replace(emote.Message, WorldObject, targetObject);

                    if (player != null)
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat(text, ChatMessageType.Broadcast));     // CreatureMessage / HearDirectSpeech?
                    break;

                case EmoteType.EraseMyQuest:
                    break;

                case EmoteType.EraseQuest:

                    if (player != null)
                        player.QuestManager.Erase(emote.Message);
                    break;

                case EmoteType.FellowBroadcast:

                    if (player != null)
                    {
                        var fellowship = player.Fellowship;
                        if (fellowship == null)
                        {
                            text = Replace(emote.Message, WorldObject, player);
                            player.Session.Network.EnqueueSend(new GameMessageSystemChat(text, ChatMessageType.Broadcast));
                        }
                        else
                        {
                            foreach (var fellow in fellowship.FellowshipMembers)
                            {
                                text = Replace(emote.Message, WorldObject, fellow);
                                fellow.Session.Network.EnqueueSend(new GameMessageSystemChat(text, ChatMessageType.Broadcast));
                            }
                        }
                    }
                    break;

                case EmoteType.Generate:

                    // unfinished - unused in PY16?
                    var wcid = (uint)emote.WeenieClassId;
                    var item = WorldObjectFactory.CreateNewWorldObject((wcid));
                    
                    break;

                case EmoteType.Give:

                    bool success = false;
                    if (player != null && emote.WeenieClassId != null)
                    {
                        item = WorldObjectFactory.CreateNewWorldObject((uint)emote.WeenieClassId);
                        var stackSize = emote.StackSize ?? 1;
                        var stackMsg = "";
                        if (stackSize > 1)
                        {
                            item.StackSize = (ushort)stackSize;
                            stackMsg = stackSize + " ";     // pluralize?
                        }
                        success = player.TryCreateInInventoryWithNetworking(item);

                        // transaction / rollback on failure?
                        if (success)
                        {
                            var msg = new GameMessageSystemChat($"{WorldObject.Name} gives you {stackMsg}{item.Name}.", ChatMessageType.Broadcast);
                            var sound = new GameMessageSound(player.Guid, Sound.ReceiveItem, 1);
                            player.Session.Network.EnqueueSend(msg, sound);
                        }
                    }
                    break;

                /* redirects to the GotoSet category for this action */
                case EmoteType.Goto:

                    var gotoSet = GetEmoteSet(EmoteCategory.GotoSet, emote.Message);
                    ExecuteEmoteSet(gotoSet, targetObject, actionChain);
                    break;

                /* increments a PropertyInt stat by some amount */
                case EmoteType.IncrementIntStat:

                    if (targetObject != null && emote.Stat != null)
                    {
                        var intProperty = (PropertyInt)emote.Stat;
                        var current = targetObject.GetProperty(intProperty) ?? 0;
                        current += emote.Amount ?? 0;
                        targetObject.SetProperty(intProperty, current);

                        if (player != null)
                            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, intProperty, current));
                    }
                    break;

                case EmoteType.IncrementMyQuest:
                    break;

                case EmoteType.IncrementQuest:

                    if (player != null)
                        player.QuestManager.Increment(emote.Message);     // kill task?
                    break;

                case EmoteType.InflictVitaePenalty:
                    if (player != null) player.VitaeCpPool++;   // TODO: full path
                    break;

                case EmoteType.InqAttributeStat:

                    if (targetCreature != null)
                    {
                        var attr = targetCreature.Attributes[(PropertyAttribute)emote.Stat];
                        success = attr != null && attr.Ranks >= emote.Min && attr.Ranks <= emote.Max;
                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqBoolStat:

                    // This is only used with NPC's 24944 and 6386, which are dev tester npc's. Not worth the current effort.
                    // Could also be post-ToD
                    break;

                case EmoteType.InqContractsFull:

                    // not part of the game at PY16?
                    //if (player != null)
                    //{
                    //    var contracts = player.TrackedContracts;
                    //    InqCategory(contracts.Count != 0 ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote);
                    //}
                    break;

                case EmoteType.InqEvent:

                    var started = EventManager.IsEventStarted(emote.Message);
                    ExecuteEmoteSet(started ? EmoteCategory.EventSuccess : EmoteCategory.EventFailure, emote.Message, targetObject, actionChain);
                    break;

                case EmoteType.InqFellowNum:

                    // unused in PY16 - ensure # of fellows between min-max?
                    ExecuteEmoteSet(player != null && player.Fellowship != null ? EmoteCategory.TestSuccess : EmoteCategory.TestNoFellow, emote.Message, targetObject, actionChain);
                    break;

                case EmoteType.InqFellowQuest:

                    // focusing on 1 person quests to begin with
                    break;

                case EmoteType.InqFloatStat:

                    if (targetObject != null)
                    {
                        var stat = targetObject.GetProperty((PropertyFloat)emote.Stat) ?? 0.0f;
                        success = stat >= emote.MinDbl && stat <= emote.MaxDbl;

                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);

                    }
                    break;

                case EmoteType.InqInt64Stat:

                    if (targetObject != null)
                    {
                        var stat = targetObject.GetProperty((PropertyInt64)emote.Stat) ?? 0;
                        success = stat >= emote.Min && stat <= emote.Max;

                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqIntStat:

                    if (targetObject != null)
                    {
                        var stat = targetObject.GetProperty((PropertyInt)emote.Stat) ?? 0;
                        success = stat >= emote.Min && stat <= emote.Max;

                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqMyQuest:
                    break;
                case EmoteType.InqMyQuestBitsOff:
                    break;
                case EmoteType.InqMyQuestBitsOn:
                    break;
                case EmoteType.InqMyQuestSolves:
                    break;
                case EmoteType.InqNumCharacterTitles:

                    //if (player != null)
                        //InqCategory(player.NumCharacterTitles != 0 ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote);
                    break;

                case EmoteType.InqOwnsItems:

                    //if (player != null)
                        //InqCategory(player.Inventory.Count > 0 ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote);
                    break;

                case EmoteType.InqPackSpace:

                    //if (player != null)
                    //{
                    //    var freeSpace = player.ContainerCapacity > player.ItemCapacity;
                    //    InqCategory(freeSpace ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote);
                    //}
                    break;

                case EmoteType.InqQuest:

                    if (player != null)
                    {
                        var hasQuest = player.QuestManager.HasQuest(emote.Message);
                        var canSolve = player.QuestManager.CanSolve(emote.Message);

                        // verify: QuestSuccess = player has quest, and their last completed time + quest minDelta <= currentTime
                        success = hasQuest && !canSolve;

                        ExecuteEmoteSet(success ? EmoteCategory.QuestSuccess : EmoteCategory.QuestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqQuestBitsOff:
                    break;
                case EmoteType.InqQuestBitsOn:
                    break;
                case EmoteType.InqQuestSolves:

                    if (player != null)
                    {
                        var questSolves = player.QuestManager.HasQuestSolves(emote.Message, emote.Min, emote.Max);

                        ExecuteEmoteSet(questSolves ? EmoteCategory.QuestSuccess : EmoteCategory.QuestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqRawAttributeStat:

                    if (targetCreature != null)
                    {
                        var attr = targetCreature.Attributes[(PropertyAttribute)emote.Stat];
                        success = attr != null && attr.Base >= emote.Min && attr.Base <= emote.Max;
                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqRawSecondaryAttributeStat:

                    if (targetCreature != null)
                    {
                        var vital = targetCreature.Vitals[(PropertyAttribute2nd)emote.Stat];
                        success = vital != null && vital.Base >= emote.Min && vital.Base <= emote.Max;
                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqRawSkillStat:

                    if (targetCreature != null)
                    {
                        var skill = targetCreature.GetCreatureSkill((Skill)emote.Stat);
                        success = skill != null && skill.Base >= emote.Min && skill.Base <= emote.Max;
                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqSecondaryAttributeStat:

                    if (targetCreature != null)
                    {
                        var vital = targetCreature.Vitals[(PropertyAttribute2nd)emote.Stat];
                        success = vital != null && vital.Ranks >= emote.Min && vital.Ranks <= emote.Max;
                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqSkillSpecialized:

                    if (targetCreature != null)
                    {
                        var skill = targetCreature.GetCreatureSkill((Skill)emote.Stat);
                        success = skill.AdvancementClass == SkillAdvancementClass.Specialized;

                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqSkillStat:

                    if (targetCreature != null)
                    {
                        var skill = targetCreature.GetCreatureSkill((Skill)emote.Stat);
                        success = skill != null && skill.Ranks >= emote.Min && skill.Ranks <= emote.Max;

                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqSkillTrained:

                    if (targetCreature != null)
                    {
                        var skill = targetCreature.GetCreatureSkill((Skill)emote.Stat);
                        success = skill.AdvancementClass >= SkillAdvancementClass.Trained;

                        // TestNoQuality?
                        ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.InqStringStat:

                    if (targetCreature != null)
                    {
                        // rarely used, only in test data?
                        if (Enum.TryParse(emote.TestString, true, out PropertyString propStr))
                        {
                            var stat = targetCreature.GetProperty(propStr);
                            success = stat.Equals(emote.Message);

                            ExecuteEmoteSet(success ? EmoteCategory.TestSuccess : EmoteCategory.TestFailure, emote.Message, targetObject, actionChain);
                        }
                    }
                    break;

                case EmoteType.InqYesNo:
                    ConfirmationManager.ProcessConfirmation((uint)emote.Stat, true);
                    break;

                case EmoteType.Invalid:
                    break;

                case EmoteType.KillSelf:

                    if (targetCreature != null)
                        targetCreature.Smite(targetCreature);
                    break;

                case EmoteType.LocalBroadcast:

                    message = Replace(emote.Message, WorldObject, targetObject);
                    WorldObject.EnqueueBroadcast(new GameMessageSystemChat(message, ChatMessageType.Broadcast));
                    break;

                case EmoteType.LocalSignal:
                    break;

                case EmoteType.LockFellow:

                    if (player != null && player.Fellowship != null)
                        player.HandleActionFellowshipChangeOpenness(false);

                    break;

                /* plays an animation on the target object (usually the player) */
                case EmoteType.ForceMotion:

                    var motionCommand = MotionCommandHelper.GetMotion(emote.Motion.Value);
                    var motion = new Motion(targetObject, motionCommand, emote.Extent);
                    targetObject.EnqueueBroadcastMotion(motion);
                    break;

                /* plays an animation on the source object */
                case EmoteType.Motion:

                    // are there players within emote range?
                    if (!WorldObject.PlayersInRange(ClientMaxAnimRange))
                        return;

                    if (WorldObject == null || WorldObject.CurrentMotionState == null) break;

                    // TODO: refactor me!
                    if (emoteSet.Category != (uint)EmoteCategory.Vendor && emoteSet.Style != null)
                    {
                        var startingMotion = new Motion((MotionStance)emoteSet.Style, (MotionCommand)emoteSet.Substyle);
                        motion = new Motion((MotionStance)emoteSet.Style, (MotionCommand)emote.Motion, emote.Extent);

                        if (WorldObject.CurrentMotionState.Stance != startingMotion.Stance)
                        {
                            if (WorldObject.CurrentMotionState.Stance == MotionStance.Invalid)
                            {
                                if (Debug)
                                    Console.WriteLine($"{WorldObject.Name} running starting motion {(MotionStance)emoteSet.Style}, {(MotionCommand)emoteSet.Substyle}");

                                WorldObject.ExecuteMotion(startingMotion);
                            }
                        }
                        else
                        {
                            if (WorldObject.CurrentMotionState.MotionState.ForwardCommand == startingMotion.MotionState.ForwardCommand)
                            {
                                if (Debug)
                                    Console.WriteLine($"{WorldObject.Name} running motion {(MotionStance)emoteSet.Style}, {(MotionCommand)emote.Motion}");

                                float? maxRange = ClientMaxAnimRange;
                                if (MotionQueue.Contains((MotionCommand)emote.Motion))
                                    maxRange = null;

                                var motionTable = DatManager.PortalDat.ReadFromDat<DatLoader.FileTypes.MotionTable>(WorldObject.MotionTableId);
                                var animLength = motionTable.GetAnimationLength(WorldObject.CurrentMotionState.Stance, (MotionCommand)emote.Motion, MotionCommand.Ready);

                                WorldObject.ExecuteMotion(motion, true, maxRange);

                                var motionChain = new ActionChain();
                                motionChain.AddDelaySeconds(animLength);
                                motionChain.AddAction(WorldObject, () =>
                                {
                                    // FIXME: this needs to be figured out better
                                    var cycles = new List<MotionCommand>()
                                    {
                                        MotionCommand.Sleeping,
                                        MotionCommand.Sitting,
                                        MotionCommand.SnowAngelState
                                    };

                                    if (!cycles.Contains(WorldObject.CurrentMotionState.MotionState.ForwardCommand))
                                    {
                                        if (Debug)
                                            Console.WriteLine($"{WorldObject.Name} running starting motion again {(MotionStance)emoteSet.Style}, {(MotionCommand)emoteSet.Substyle}");

                                        WorldObject.ExecuteMotion(startingMotion);
                                    }
                                });
                                motionChain.EnqueueChain();

                                // append time to current chain
                                NextAvailable += TimeSpan.FromSeconds(animLength);
                                if (Debug)
                                    Console.WriteLine($"{WorldObject.Name} appending time to existing chain: " + animLength);
                            }
                        }
                    }
                    else
                    {
                        // vendor / other motions
                        var startingMotion = new Motion(MotionStance.NonCombat, MotionCommand.Ready);
                        var motionTable = DatManager.PortalDat.ReadFromDat<DatLoader.FileTypes.MotionTable>(WorldObject.MotionTableId);
                        var animLength = motionTable.GetAnimationLength(WorldObject.CurrentMotionState.Stance, (MotionCommand)emote.Motion, MotionCommand.Ready);

                        motion = new Motion(MotionStance.NonCombat, (MotionCommand)emote.Motion, emote.Extent);

                        if (Debug)
                            Console.WriteLine($"{WorldObject.Name} running motion (block 2) {MotionStance.NonCombat}, {(MotionCommand)(emote.Motion ?? 0)}");

                        WorldObject.ExecuteMotion(motion);

                        var motionChain = new ActionChain();
                        motionChain.AddDelaySeconds(animLength);
                        motionChain.AddAction(WorldObject, () => WorldObject.ExecuteMotion(startingMotion, false));

                        motionChain.EnqueueChain();
                    }

                    break;

                /* move to position relative to home */
                case EmoteType.Move:

                    if (creature != null)
                    {
                        var newPos = new Position(creature.Home);
                        newPos.Pos += new Vector3(emote.OriginX ?? 0, emote.OriginY ?? 0, emote.OriginZ ?? 0);      // uses relative offsets
                        newPos.Rotation *= new Quaternion(emote.AnglesX ?? 0, emote.AnglesY ?? 0, emote.AnglesZ ?? 0, emote.AnglesW ?? 1);  // also relative?

                        if (Debug)
                            Console.WriteLine(newPos.ToLOCString());

                        // get new cell
                        newPos.LandblockId = new LandblockId(PositionExtensions.GetCell(newPos));

                        creature.MoveTo(newPos, creature.GetRunRate());
                    }
                    break;

                case EmoteType.MoveHome:

                    if (Debug)
                        Console.WriteLine(creature.Home.ToLOCString());

                    // TODO: call MoveToManager on server
                    if (creature != null && creature.Home != null)      // home seems to be null for creatures?
                        creature.MoveTo(creature.Home, creature.GetRunRate());
                    break;

                case EmoteType.MoveToPos:

                    if (creature != null)
                    {
                        var currentPos = creature.Location;

                        var newPos = new Position();
                        newPos.LandblockId = new LandblockId(currentPos.LandblockId.Raw);
                        newPos.Pos = new Vector3(emote.OriginX ?? currentPos.Pos.X, emote.OriginY ?? currentPos.Pos.Y, emote.OriginZ ?? currentPos.Pos.Z);

                        if (emote.AnglesX == null || emote.AnglesY == null || emote.AnglesZ == null || emote.AnglesW == null)
                            newPos.Rotation = new Quaternion(currentPos.Rotation.X, currentPos.Rotation.Y, currentPos.Rotation.Z, currentPos.Rotation.W);
                        else
                            newPos.Rotation = new Quaternion(emote.AnglesX ?? 0, emote.AnglesY ?? 0, emote.AnglesZ ?? 0, emote.AnglesW ?? 1);

                        if (emote.ObjCellId != null)
                            newPos.LandblockId = new LandblockId(emote.ObjCellId.Value);

                        creature.MoveTo(newPos, creature.GetRunRate());
                    }
                    break;

                case EmoteType.OpenMe:

                    WorldObject.Open(WorldObject);
                    break;

                case EmoteType.PetCastSpellOnOwner:

                    if (creature != null)
                        creature.CreateCreatureSpell(targetObject.Guid, (uint)emote.SpellId);
                    break;

                case EmoteType.PhysScript:

                    // TODO: landblock broadcast
                    WorldObject.PhysicsObj.play_script((PlayScript)emote.PScript, 1.0f);
                    break;

                case EmoteType.PopUp:
                    if (player != null)
                    {
                        if ((ConfirmationType)emote.Stat == ConfirmationType.Undefined)
                            player.Session.Network.EnqueueSend(new GameEventPopupString(player.Session, emote.Message));
                        else
                        {
                            Confirmation confirm = new Confirmation((ConfirmationType)emote.Stat, emote.Message, WorldObject, targetObject);
                            ConfirmationManager.AddConfirmation(confirm);
                            player.Session.Network.EnqueueSend(new GameEventConfirmationRequest(player.Session, (ConfirmationType)emote.Stat, confirm.ConfirmationID, confirm.Message));
                        }
                    }
                    break;

                case EmoteType.RemoveContract:

                    if (player != null)
                        player.HandleActionAbandonContract((uint)emote.Stat);
                    break;

                case EmoteType.RemoveVitaePenalty:

                    if (player != null) player.VitaeCpPool = 0;     // TODO: call full path
                    break;

                case EmoteType.ResetHomePosition:

                    //creature = sourceObject as Creature;
                    //if (creature != null)
                    //    creature.Home = emoteAction.Position;
                    break;

                case EmoteType.Say:

                    WorldObject.EnqueueBroadcast(new GameMessageCreatureMessage(emote.Message, WorldObject.Name, WorldObject.Guid.Full, ChatMessageType.Emote));
                    break;

                case EmoteType.SetAltRacialSkills:
                    break;

                case EmoteType.SetBoolStat:
                    targetObject.SetProperty((PropertyBool)emote.Stat, emote.Amount == 0 ? false : true);
                    break;

                case EmoteType.SetEyePalette:
                    if (creature != null)
                        creature.EyesPaletteDID = (uint)emote.Display;
                    break;

                case EmoteType.SetEyeTexture:
                    if (creature != null)
                        creature.EyesTextureDID = (uint)emote.Display;
                    break;

                case EmoteType.SetFloatStat:
                    targetObject.SetProperty((PropertyFloat)emote.Stat, (float)emote.Amount);
                    break;

                case EmoteType.SetHeadObject:
                    if (creature != null)
                        creature.HeadObjectDID = (uint)emote.Display;
                    break;

                case EmoteType.SetHeadPalette:
                    break;

                case EmoteType.SetInt64Stat:
                    player.SetProperty((PropertyInt64)emote.Stat, (int)emote.Amount);
                    break;

                case EmoteType.SetIntStat:
                    player.SetProperty((PropertyInt)emote.Stat, (int)emote.Amount);
                    break;

                case EmoteType.SetMouthPalette:
                    break;

                case EmoteType.SetMouthTexture:
                    if (creature != null)
                        creature.MouthTextureDID = (uint)emote.Display;
                    break;

                case EmoteType.SetMyQuestBitsOff:
                    break;
                case EmoteType.SetMyQuestBitsOn:
                    break;
                case EmoteType.SetMyQuestCompletions:
                    break;
                case EmoteType.SetNosePalette:
                    break;

                case EmoteType.SetNoseTexture:
                    if (creature != null)
                        creature.NoseTextureDID = (uint)emote.Display;
                    break;

                case EmoteType.SetQuestBitsOff:
                    break;
                case EmoteType.SetQuestBitsOn:
                    break;
                case EmoteType.SetQuestCompletions:
                    break;
                case EmoteType.SetSanctuaryPosition:

                    if (player != null && emote.ObjCellId.HasValue && emote.OriginX.HasValue && emote.OriginY.HasValue && emote.OriginZ.HasValue && emote.AnglesW.HasValue && emote.AnglesX.HasValue && emote.AnglesY.HasValue && emote.AnglesZ.HasValue)
                        player.Sanctuary = new Position(emote.ObjCellId.Value, emote.OriginX.Value, emote.OriginY.Value, emote.OriginZ.Value, emote.AnglesX.Value, emote.AnglesY.Value, emote.AnglesZ.Value, emote.AnglesW.Value);
                    break;

                case EmoteType.Sound:

                    WorldObject.EnqueueBroadcast(new GameMessageSound(WorldObject.Guid, (Sound)emote.Sound, 1.0f));
                    break;

                case EmoteType.SpendLuminance:
                    if (player != null)
                        player.SpendLuminance((long)emote.Amount);
                    break;

                case EmoteType.StampFellowQuest:
                    break;
                case EmoteType.StampMyQuest:
                    break;
                case EmoteType.StampQuest:

                    // work needs to be done here
                    if (player != null)
                        player.QuestManager.Stamp(emote.Message);
                    break;

                case EmoteType.StartBarber:
                    break;

                case EmoteType.StartEvent:

                    EventManager.StartEvent(emote.Message);
                    break;

                case EmoteType.StopEvent:

                    EventManager.StopEvent(emote.Message);
                    break;

                case EmoteType.TakeItems:

                    if (player != null && emote.WeenieClassId != null)
                    {
                        item = WorldObjectFactory.CreateNewWorldObject((uint)emote.WeenieClassId);
                        if (item == null) break;

                        success = player.TryRemoveItemFromInventoryWithNetworkingWithDestroy(item, (ushort)emote.Amount);
                    }
                    break;

                case EmoteType.TeachSpell:

                    if (player != null)
                        player.LearnSpellWithNetworking((uint)emote.SpellId);
                    break;

                case EmoteType.TeleportSelf:

                    //if (WorldObject is Player)
                        //(WorldObject as Player).Teleport(emote.Position);
                    break;

                case EmoteType.TeleportTarget:

                    //if (player != null)
                        //player.Teleport(emote.Position);
                    break;

                case EmoteType.Tell:

                    if (player != null)
                    {
                        message = Replace(emote.Message, WorldObject, player);
                        player.Session.Network.EnqueueSend(new GameMessageHearDirectSpeech(WorldObject, message, player, ChatMessageType.Tell));
                    }
                    break;

                case EmoteType.TellFellow:

                    if (player != null)
                    {
                        var fellowship = player.Fellowship;
                        if (fellowship == null)
                        {
                            message = Replace(emote.Message, WorldObject, player);
                            player.Session.Network.EnqueueSend(new GameMessageHearDirectSpeech(WorldObject, message, player, ChatMessageType.Tell));
                        }
                        else
                        {
                            foreach (var fellow in fellowship.FellowshipMembers)
                            {
                                message = Replace(emote.Message, WorldObject, fellow);
                                player.Session.Network.EnqueueSend(new GameMessageHearDirectSpeech(WorldObject, message, fellow, ChatMessageType.Tell));
                            }
                        }
                    }
                    break;

                case EmoteType.TextDirect:

                    if (player != null)
                    {
                        message = Replace(emote.Message, WorldObject, player);
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat(message, ChatMessageType.Broadcast));
                    }
                    break;

                case EmoteType.Turn:

                    if (creature != null)
                    {
                        // turn to heading
                        var rotation = new Quaternion(emote.AnglesX ?? 0, emote.AnglesY ?? 0, emote.AnglesZ ?? 0, emote.AnglesW ?? 1);
                        var newPos = new Position(creature.Location);
                        newPos.Rotation = rotation;

                        var rotateTime = creature.TurnTo(newPos);
                        //actionChain.AddDelaySeconds(rotateTime);    // todo: adding delays in the middle of actions
                    }
                    break;

                case EmoteType.TurnToTarget:

                    if (creature != null && targetCreature != null)
                    {
                        var rotateTime = creature.Rotate(targetCreature);
                        //actionChain.AddDelaySeconds(rotateTime);    // todo: adding delays in the middle of actions
                    }
                    break;

                case EmoteType.UntrainSkill:

                    if (player != null)
                        player.UntrainSkill((Skill)emote.Stat, 1);
                    break;

                case EmoteType.UpdateFellowQuest:
                    break;
                case EmoteType.UpdateMyQuest:
                    break;
                case EmoteType.UpdateQuest:

                    // is this only for solving??

                    // only delay seems to be with test NPC here
                    // still, unsafe to use any emotes directly outside of a chain,
                    // as they could be executed out-of-order
                    if (player != null)
                    {
                        var questName = emote.Message;
                        player.QuestManager.Update(questName);
                        var hasQuest = player.QuestManager.HasQuest(questName);
                        ExecuteEmoteSet(hasQuest ? EmoteCategory.QuestSuccess : EmoteCategory.QuestFailure, emote.Message, targetObject, actionChain);
                    }
                    break;

                case EmoteType.WorldBroadcast:

                    message = Replace(text, WorldObject, targetObject);

                    var onlinePlayers = PlayerManager.GetAllOnline();

                    foreach (var session in onlinePlayers)
                        session.Session.Network.EnqueueSend(new GameMessageSystemChat(message, ChatMessageType.WorldBroadcast));

                    break;

                default:
                    log.Debug($"EmoteManager.Execute - Encountered Unhandled EmoteType {(EmoteType)emote.Type} for {WorldObject.Name} ({WorldObject.WeenieClassId})");
                    break;
            }
        }

        /// <summary>
        /// Selects an emote set based on category, and optional: quest, vendor, rng
        /// </summary>
        public BiotaPropertiesEmote GetEmoteSet(EmoteCategory category, string questName = null, VendorType? vendorType = null, uint? wcid = null, bool useRNG = true)
        {
            var emoteSet = WorldObject.Biota.BiotaPropertiesEmote.Where(e => e.Category == (uint)category);

            // optional criteria
            if (questName != null)
                emoteSet = emoteSet.Where(e => e.Quest.Equals(questName, StringComparison.OrdinalIgnoreCase));
            if (vendorType != null)
                emoteSet = emoteSet.Where(e => e.VendorType != null && e.VendorType.Value == (uint)vendorType);
            if (wcid != null)
                emoteSet = emoteSet.Where(e => e.WeenieClassId == wcid.Value);
            if (useRNG)
                emoteSet = emoteSet.Where(e => e.Probability >= ThreadSafeRandom.Next(0.0f, 1.0f));

            return emoteSet.FirstOrDefault();
        }

        /// <summary>
        /// Convenience wrapper between GetEmoteSet and ExecututeEmoteSet
        /// </summary>
        public bool ExecuteEmoteSet(EmoteCategory category, string quest = null, WorldObject targetObject = null, ActionChain actionChain = null, bool forceEnqueue = false)
        {
            var emoteSet = GetEmoteSet(category, quest);
            return ExecuteEmoteSet(emoteSet, targetObject, actionChain, forceEnqueue);
        }

        /// <summary>
        /// Executes a set of emotes to run with delays
        /// </summary>
        /// <param name="emoteSet">A list of emotes to execute</param>
        /// <param name="targetObject">An optional target, usually player</param>
        /// <param name="actionChain">For adding delays between emotes</param>
        public bool ExecuteEmoteSet(BiotaPropertiesEmote emoteSet, WorldObject targetObject = null, ActionChain actionChain = null, bool firstAction = false)
        {
            if (emoteSet == null) return false;

            bool enqueue = (actionChain == null);

            // detect busy state
            // TODO: maybe eventually we should consider having categories that can be queued?
            // there are some categories that shouldn't be queued, like heartbeats...
            var currentTime = DateTime.UtcNow;
            if ((enqueue || firstAction) && currentTime < NextAvailable)
            {
                if (Debug && emoteSet.Category != (uint)EmoteCategory.HeartBeat)
                    Console.WriteLine($"{WorldObject.Name}.ExecuteEmoteSet({(EmoteCategory)emoteSet.Category}): busy for another {(NextAvailable - currentTime).TotalSeconds}s");

                return false;
            }

            // start building a new action chain if needed
            if (actionChain == null)
                actionChain = new ActionChain();

            // build action chain for each emote
            var totalDelay = 0.0f;
            foreach (var emote in emoteSet.BiotaPropertiesEmoteAction)
            {
                totalDelay += emote.Delay;
                actionChain.AddDelaySeconds(emote.Delay);
                actionChain.AddAction(WorldObject, () => ExecuteEmote(emoteSet, emote, targetObject, actionChain));
            }

            // set next available time
            if (enqueue || firstAction)
            {
                NextAvailable = currentTime + TimeSpan.FromSeconds(totalDelay);

                var threshold = 1.5f;
                if (Debug && totalDelay >= threshold)
                    Console.WriteLine($"{WorldObject.Name}.ExecuteEmoteSet({(EmoteCategory)emoteSet.Category}): added {emoteSet.BiotaPropertiesEmoteAction.Count()} emotes @ {totalDelay}s");
            }

            // enqueue if top of chain
            if (enqueue)
                actionChain.EnqueueChain();

            return true;
        }

        /// <summary>
        /// The maximum animation range of the client
        /// Motions broadcast outside of this range will be automatically queued by client
        /// </summary>
        public static float ClientMaxAnimRange = 96.0f;     // verify: same indoors?

        /// <summary>
        /// The client automatically queues animations that are broadcast outside of 96.0f range
        /// Normally we exclude these emotes from being broadcast outside this range,
        /// but for certain emotes (like monsters going to sleep) we want to always broadcast / enqueue
        /// </summary>
        public static HashSet<MotionCommand> MotionQueue = new HashSet<MotionCommand>()
        {
            MotionCommand.Sleeping
        };

        public void DoVendorEmote(VendorType vendorType, WorldObject target)
        {
            var actionChain = new ActionChain();

            var vendorSet = GetEmoteSet(EmoteCategory.Vendor, null, vendorType);
            var heartbeatSet = GetEmoteSet(EmoteCategory.Vendor, null, VendorType.Heartbeat);

            ExecuteEmoteSet(vendorSet, target, actionChain);
            ExecuteEmoteSet(heartbeatSet, target, actionChain);

            actionChain.EnqueueChain();
        }

        public IEnumerable<BiotaPropertiesEmote> Emotes(EmoteCategory emoteCategory)
        {
            return WorldObject.Biota.BiotaPropertiesEmote.Where(x => x.Category == (int)emoteCategory);
        }

        public string Replace(string message, WorldObject source, WorldObject target)
        {
            var result = message;

            var sourceName = source != null ? source.Name : "";
            var targetName = target != null ? target.Name : "";

            result = result.Replace("%n", sourceName);
            result = result.Replace("%mn", sourceName);
            result = result.Replace("%s", targetName);
            result = result.Replace("%tn", targetName);
            result = result.Replace("%tqt", "some amount of time");

            return result;
        }

        public void HeartBeat()
        {
            // player didn't do idle emotes in retail?
            if (WorldObject is Player)
                return;

            ExecuteEmoteSet(EmoteCategory.HeartBeat);
        }

        public void OnUse(Creature activator)
        {
            ExecuteEmoteSet(EmoteCategory.Use, null, activator);
        }

        public void OnActivation(Creature activator)
        {
            ExecuteEmoteSet(EmoteCategory.Activation, null, activator);
        }

        public void OnWield(Creature wielder)
        {
            ExecuteEmoteSet(EmoteCategory.Wield, null, wielder);
        }

        public void OnUnwield(Creature wielder)
        {
            ExecuteEmoteSet(EmoteCategory.UnWield, null, wielder);
        }

        public void OnAttack(Creature attacker)
        {
            ExecuteEmoteSet(EmoteCategory.NewEnemy, null, attacker);
        }

        public void OnDeath(DamageHistory damageHistory)
        {
            ExecuteEmoteSet(EmoteCategory.KillTaunt, null, damageHistory.TopDamager);

            foreach (var damager in damageHistory.Damagers)
                ExecuteEmoteSet(EmoteCategory.Death, null, damager);

            if (damageHistory.Damagers.Count == 0)
                ExecuteEmoteSet(EmoteCategory.Death, null, null);
        }
    }
}
