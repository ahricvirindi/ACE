using ACE.Database.Models.Shard;
using ACE.WorldBuilder.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Character = ACE.WorldBuilder.Entity.Character;

namespace ACE.WorldBuilder.Managers
{
    public static class CharacterManager
    {

        public static List<Character> GetAllCharacters()
        {
            var allBiotas = ACE.Database.DatabaseManager.Shard.GetAllPlayerBiotasInParallel();
            var allCharacters = ProjectFromPlayerBiota(allBiotas);
            return allCharacters;
        }

        private static List<Character> ProjectFromPlayerBiota(List<Biota> biotaList)
        {
            var characters = new List<Character>();

            foreach(var biota in biotaList)
            {
                characters.Add(ProjectFromPlayerBiota(biota));
            }

            return characters;
        }

        private static Character ProjectFromPlayerBiota(Biota biota)
        {
            var c = new Character()
            {
                AccountId = 0,
                Level = biota.BiotaPropertiesInt.FirstOrDefault(x => x.Type == (uint)ACE.Entity.Enum.Properties.PropertyInt.Level)?.Value,
                Id = biota.Id,
                Name = biota.GetProperty(ACE.Entity.Enum.Properties.PropertyString.Name)
            };

            return c;
        }
    }
}
