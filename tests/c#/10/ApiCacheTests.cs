﻿using System.Linq;
using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.APICache;

public class ResolveWeaponSkills {
	[Fact]
	public async Task ResolveWeaponSkills2h() {
		var code = new BuildCode() {
			Profession = Profession.Necromancer,
			WeaponSet1 = {
				MainHand = WeaponType.Staff,
			}
		};

		var effective = Static.ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		Assert.Equal(WeaponType.Staff, effective.MainHand);
		Assert.Equal(WeaponType._UNDEFINED, effective.OffHand);

		var reference = new SkillId[5] { SkillId.Necrotic_Grasp, SkillId.Mark_of_Blood, SkillId.Chillblains, SkillId.Putrid_Mark, SkillId.Reapers_Mark };

		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, effective, i));
	}

	[Fact]
	public async Task ResolveWeaponSkillsNormal() {
		var code = new BuildCode() {
			Profession = Profession.Necromancer,
			WeaponSet1 = {
				MainHand = WeaponType.Dagger,
				OffHand = WeaponType.Dagger,
			}
		};

		var effective = Static.ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		Assert.Equal(WeaponType.Dagger, effective.MainHand);
		Assert.Equal(WeaponType.Dagger, effective.OffHand);

		var reference = new SkillId[5] { SkillId.Necrotic_Slash, SkillId.Life_Siphon, SkillId.Dark_Pact, SkillId.Deathly_Swarm, SkillId.Enfeebling_Blood };

		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, effective, i));
	}

	[Fact]
	public async Task ResolveWeaponSkillsFromOtherSet()
	{
		var code = new BuildCode() {
			Profession = Profession.Necromancer,
			WeaponSet1 = {
				MainHand = WeaponType.Dagger,
				Sigil1 = ItemId.Superior_Sigil_of_Deamons2,
			}, 
			WeaponSet2 = {
				OffHand = WeaponType.Dagger,
				Sigil2 = ItemId.Superior_Sigil_of_Concentration2,
			}
		};

		var effective = Static.ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		Assert.Equal(WeaponType.Dagger, effective.MainHand);
		Assert.Equal(ItemId.Superior_Sigil_of_Deamons2, effective.Sigil1);
		Assert.Equal(WeaponType.Dagger, effective.OffHand);
		Assert.Equal(ItemId.Superior_Sigil_of_Concentration2, effective.Sigil2);


		var reference = new SkillId[5] { SkillId.Necrotic_Slash, SkillId.Life_Siphon, SkillId.Dark_Pact, SkillId.Deathly_Swarm, SkillId.Enfeebling_Blood };

		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, effective, i));
	}

	[Fact]
	public async Task ResolveWeaponSkillsFromOtherSetExcept2h()
	{
		var code = new BuildCode() {
			Profession = Profession.Necromancer,
			WeaponSet1 = {
				MainHand = WeaponType.Dagger,
				Sigil1 = ItemId.Superior_Sigil_of_Deamons2,
			},
			WeaponSet2 = {
				MainHand = WeaponType.Staff,
				Sigil2 = ItemId.Superior_Sigil_of_Concentration2,
			}
		};

		var effective = Static.ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		Assert.Equal(WeaponType.Dagger, effective.MainHand);
		Assert.Equal(ItemId.Superior_Sigil_of_Deamons2, effective.Sigil1);
		Assert.Equal(WeaponType._UNDEFINED, effective.OffHand);
		Assert.Equal(ItemId._UNDEFINED, effective.Sigil2);


		var reference = new SkillId[5] { SkillId.Necrotic_Slash, SkillId.Life_Siphon, SkillId.Dark_Pact, SkillId._UNDEFINED, SkillId._UNDEFINED };

		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, effective, i));
	}
}