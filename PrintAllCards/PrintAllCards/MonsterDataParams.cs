using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrintAllCards
{
    [Serializable]
    public class MonsterDataParams
    {
        public string Name;

        public string ArtistName;

        public string Description;

        public List<string> CardExpansionTypes;

        public string ElementIndex;

        public string Rarity;

        public string MonsterType;

        public string NextEvolution;

        public string PreviousEvolution;

        public List<string> Roles;

        public Stats BaseStats;

        public List<string> SkillList;
        public MonsterDataParams(MonsterData monsterData)
        {
            this.Name = monsterData.Name;
            this.ArtistName = monsterData.ArtistName;
            this.Description = monsterData.Description;
            this.CardExpansionTypes = new List<string>();
            this.ElementIndex = Enum.GetName(typeof(EElementIndex), monsterData.ElementIndex);
            this.Rarity = Enum.GetName(typeof(ERarity), monsterData.Rarity);
            this.MonsterType = Enum.GetName(typeof(EMonsterType), monsterData.MonsterType);
            this.NextEvolution = Enum.GetName(typeof(EMonsterType), monsterData.NextEvolution);
            this.PreviousEvolution = Enum.GetName(typeof(EMonsterType), monsterData.PreviousEvolution);
            this.Roles = monsterData.Roles.Select(c => c.ToString()).ToList();
            this.BaseStats = monsterData.BaseStats;
            this.SkillList = monsterData.SkillList.Select(c => c.ToString()).ToList();
        }
    }
}
