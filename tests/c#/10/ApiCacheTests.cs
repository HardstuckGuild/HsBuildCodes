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
				Sigil1 = ItemId.Legendary_Sigil_of_Demons,
			}, 
			WeaponSet2 = {
				OffHand = WeaponType.Dagger,
				Sigil2 = ItemId.Legendary_Sigil_of_Concentration,
			}
		};

		var effective = Static.ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		Assert.Equal(WeaponType.Dagger, effective.MainHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Demons, effective.Sigil1);
		Assert.Equal(WeaponType.Dagger, effective.OffHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Concentration, effective.Sigil2);

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
				Sigil1 = ItemId.Legendary_Sigil_of_Demons,
			},
			WeaponSet2 = {
				MainHand = WeaponType.Staff,
				Sigil2 = ItemId.Legendary_Sigil_of_Concentration,
			}
		};

		var effective = Static.ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		Assert.Equal(WeaponType.Dagger, effective.MainHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Demons, effective.Sigil1);
		Assert.Equal(WeaponType._UNDEFINED, effective.OffHand);
		Assert.Equal(ItemId._UNDEFINED, effective.Sigil2);

		var reference = new SkillId[5] { SkillId.Necrotic_Slash, SkillId.Life_Siphon, SkillId.Dark_Pact, SkillId._UNDEFINED, SkillId._UNDEFINED };

		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, effective, i));
	}

	[Fact]
	public async Task ResolveThiefWeaponSkillsDD()
	{
		var code = new BuildCode();
		code.Profession = Profession.Thief;
		code.WeaponSet1.MainHand = WeaponType.Dagger;
		code.WeaponSet1.OffHand  = WeaponType.Dagger;

		var reference = new SkillId[5] { SkillId.Double_Strike, SkillId.Heartseeker, SkillId.Death_Blossom, SkillId.Dancing_Dagger, SkillId.Cloak_and_Dagger };
		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, i));
	}

	[Fact]
	public async Task ResolveThiefWeaponSkillsNoSecond()
	{
		var code = new BuildCode();
		code.Profession = Profession.Thief;
		code.WeaponSet1.MainHand = WeaponType.Dagger;
		code.WeaponSet1.OffHand  = WeaponType._UNDEFINED;

		var reference = new SkillId[5] { SkillId.Double_Strike, SkillId.Heartseeker, SkillId.Twisting_Fangs, SkillId._UNDEFINED, SkillId._UNDEFINED };
		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, i));
	}

	[Fact]
	public async Task ResolveThiefWeaponSkillsTwohanded()
	{
		var code = new BuildCode();
		code.Profession = Profession.Thief;
		code.WeaponSet1.MainHand = WeaponType.Shortbow;

		var reference = new SkillId[5] { SkillId.Surprise_Shot, SkillId.Cluster_Bomb, SkillId.Disabling_Shot, SkillId.Choking_Gas, SkillId.Infiltrators_Arrow };
		for(int i = 0; i < reference.Length; i++)
			Assert.Equal(reference[i], await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, i));
	}

	[Fact]
	public async Task ResolveTraitId()
	{
		var code = new BuildCode(){
			Profession = Profession.Mesmer,
			Specializations = {
				Choice1 = {
					SpecializationId = SpecializationId.Dueling,
					Choices = {
						Adept = TraitLineChoice.MIDDLE,
					}
				}
			}
		};

		var id = await V2.APICache.ResolveTrait(code.Specializations.Choice1, V2.TraitSlot.Adept);

		Assert.Equal(TraitId.Desperate_Decoy, id);
	}

	[Fact] /* regression: the third skill on ele staff was not the correct attunement */
	public async Task LoadEleStaffSkills()
	{
		var code = new BuildCode();
		code.Profession = Profession.Elementalist;
		code.WeaponSet1.MainHand = WeaponType.Staff;

		var weapons = Static.ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);
		var thirdSkill = await V2.APICache.ResolveWeaponSkill(code, weapons, 2);

		Assert.Equal(SkillId.Flame_Burst, thirdSkill);
	}

	[Fact] /* guardian staff would show tome skills */
	public async Task GuardianStaff() {
		var code = new BuildCode();
		code.Profession = Profession.Guardian;
		code.WeaponSet1.MainHand = WeaponType.Staff;

		Assert.Equal(SkillId.Bolt_of_Wrath      , await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, 0));
		Assert.Equal(SkillId.Holy_Strike        , await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, 1));
		Assert.Equal(SkillId.Symbol_of_Swiftness, await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, 2));
		Assert.Equal(SkillId.Empower            , await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, 3));
		Assert.Equal(SkillId.Line_of_Warding    , await V2.APICache.ResolveWeaponSkill(code, code.WeaponSet1, 4));
	}
}
