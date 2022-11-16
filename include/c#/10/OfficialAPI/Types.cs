using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hardstuck.GuildWars2.BuildCodes.V2.OfficialAPI;

//NOTE(Rennorb): These types are incomplete and only have the filds required for internal processing.
//NOTE(Rennorb): Names match api names.

#pragma warning disable CS0649 // value is never assigned to

class Item {
	public int         Id       = 0;
	public string      Type     = string.Empty;
	public ItemDetails Details  = new();
}

class ItemDetails {
	public string        Type         = string.Empty;
	public InfixUpgrade? InfixUpgrade = null;
}

class InfixUpgrade {
	public int Id = 0;
}

class Trait {
	public int Id    = 0;
	public int Order = 0;
}

class Profession {
	public Dictionary<string, Weapon>          Weapons         = new();
	public List<int>                           Specializations = new();
	/// <remarks> schema version == 2019-12-19T00:00:00.000Z </remarks>
	[JsonConverter(typeof(PaletteConverter))]
	public List<(int paletteID, int skillId)>? SkillsByPalette = new();
}

class PaletteConverter : JsonConverter<List<(int, int)>> {
	public override List<(int, int)>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var list = new List<(int, int)>();

		Debug.Assert(reader.TokenType == JsonTokenType.StartArray); reader.Read();
		while(reader.TokenType != JsonTokenType.EndArray)
		{
			Debug.Assert(reader.TokenType == JsonTokenType.StartArray); reader.Read();
			int paletteId = reader.GetInt32(); reader.Read();
			int skillId = reader.GetInt32(); reader.Read();
			Debug.Assert(reader.TokenType == JsonTokenType.EndArray); reader.Read();
			list.Add((paletteId, skillId));
		}
		//NOTE(Rennorb): You don't read the last end object token for some reason.

		return list;
	}

	public override void Write(Utf8JsonWriter writer, List<(int, int)> value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}

class Weapon {
	public List<WeaponSkill> Skills = new();
}

class WeaponSkill {
	public int    Id   = 0;
	public string Slot = string.Empty;
}

class Specialization {
	public int       Id          = 0;
	public List<int> MajorTraits = new();
}

class TokenInfo {
	public List<string> Permissions = new();
}

class Character {
	public string             Name          = string.Empty;
	public string             Profession    = string.Empty;
	public List<BuildTab>     BuildTabs     = new();
	public List<EquipmentTab> EquipmentTabs = new();
}

class BuildTab {
	public int   Tab      = 0;
	public bool  IsActive = false;
	public Build Build    = new();
}

class Build {
	public List<BuildSpecialization> Specializations = new();
	public Skills                    Skills          = new();
	public Skills                    AquaticSkills   = new();
	public Pets?                     Pets            = null;
	public List<string?>?            Legends         = null;
	public List<string?>?            AquaticLegends  = null;
}

class Skills {
	public int?       Heal      = null;
	public List<int?> Utilities = new();
	public int?       Elite     = null;
}

class Pets {
	public List<int> Terrestrial = new();
	public List<int> Aquatic     = new();
}

class BuildSpecialization {
	public int?        Id     = null;
	public List<int?>? Traits = null;
}

class EquipmentTab {
	public int                 Id           = 0;
	public bool                IsActive     = false;
	public List<EquipmentItem> Equipment    = new();
	public PvPEquipment        EquipmentPvp = new();
}

class EquipmentItem {
	public int        Id        = 0;
	public string     Slot      = string.Empty;
	public List<int>? Upgrades  = null;
	public List<int>? Infusions = null;
	public Stats?     Stats     = null;
}

class Stats {
	public int Id = 0;
}

class PvPEquipment {
	public int?       Amulet = null;
	public int?       Rune   = null;
	public List<int?> Sigils = new();
}

#pragma warning restore CS0649
