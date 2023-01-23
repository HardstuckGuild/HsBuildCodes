import {describe, expect, test} from '@jest/globals';
import ItemId from '../../../include/ts/es6/Database/ItemIds';
import SkillId from '../../../include/ts/es6/Database/SkillIds';
import SpecializationId from '../../../include/ts/es6/Database/SpecializationIds';
import { ResolveEffectiveWeapons } from '../../../include/ts/es6/Database/Static';
import TraitId from '../../../include/ts/es6/Database/TraitIds';
import APICache from '../../../include/ts/es6/OfficialAPI/APICache';
import { BuildCode, Profession, TraitLineChoice, TraitSlot, WeaponSetNumber, WeaponType } from '../../../include/ts/es6/Structures';

describe("ResolveWeaponSkills", () => {
	test('ResolveWeaponSkillsEmpty', async () => {
		const code = new BuildCode();

		const effective = ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		expect(effective.MainHand).toBe(WeaponType._UNDEFINED);
		expect(effective.OffHand).toBe(WeaponType._UNDEFINED);

		const reference = new Array(5).fill(SkillId._UNDEFINED, 0, 5);

		for(let i = 0; i < reference.length; i++)
			expect(await APICache.ResolveWeaponSkill(code, effective, i)).toBe(reference[i]);
	});
	
	test('ResolveWeaponSkills2h', async () => {
		const code = new BuildCode();
		code.Profession = Profession.Necromancer;
		code.WeaponSet1.MainHand = WeaponType.Staff;

		const effective = ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		expect(effective.MainHand).toBe(WeaponType.Staff);
		expect(effective.OffHand).toBe(WeaponType._UNDEFINED);

		const reference = [ SkillId.Necrotic_Grasp, SkillId.Mark_of_Blood, SkillId.Chillblains, SkillId.Putrid_Mark, SkillId.Reapers_Mark ];

		for(let i = 0; i < reference.length; i++)
			expect(await APICache.ResolveWeaponSkill(code, effective, i)).toBe(reference[i]);
	});

	test('ResolveWeaponSkillsNormal', async () => {
		const code = new BuildCode();
		code.Profession = Profession.Necromancer;
		code.WeaponSet1.MainHand = WeaponType.Dagger;
		code.WeaponSet1.OffHand  = WeaponType.Dagger;

		const effective = ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		expect(effective.MainHand).toBe(WeaponType.Dagger);
		expect(effective.OffHand).toBe(WeaponType.Dagger);

		const reference = [ SkillId.Necrotic_Slash, SkillId.Life_Siphon, SkillId.Dark_Pact, SkillId.Deathly_Swarm, SkillId.Enfeebling_Blood ];

		for(let i = 0; i < reference.length; i++)
			expect(await APICache.ResolveWeaponSkill(code, effective, i)).toBe(reference[i]);
	});

	test('ResolveWeaponSkillsFromOtherSet', async () => {
		const code = new BuildCode();
		code.Profession = Profession.Necromancer;
		code.WeaponSet1.MainHand = WeaponType.Dagger;
		code.WeaponSet1.Sigil1 = ItemId.Legendary_Sigil_of_Demons;
		code.WeaponSet2.OffHand  = WeaponType.Dagger;
		code.WeaponSet2.Sigil2 = ItemId.Legendary_Sigil_of_Concentration;

		const effective = ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		expect(effective.MainHand).toBe(WeaponType.Dagger);
		expect(effective.Sigil1).toBe(ItemId.Legendary_Sigil_of_Demons);
		expect(effective.OffHand).toBe(WeaponType.Dagger);
		expect(effective.Sigil2).toBe(ItemId.Legendary_Sigil_of_Concentration);


		const reference = [ SkillId.Necrotic_Slash, SkillId.Life_Siphon, SkillId.Dark_Pact, SkillId.Deathly_Swarm, SkillId.Enfeebling_Blood ];

		for(let i = 0; i < reference.length; i++)
			expect(await APICache.ResolveWeaponSkill(code, effective, i)).toBe(reference[i]);
	});

	test('ResolveWeaponSkillsFromOtherSetExcept2h', async () => {
		const code = new BuildCode();
		code.Profession = Profession.Necromancer;
		code.WeaponSet1.MainHand = WeaponType.Dagger;
		code.WeaponSet1.Sigil1 = ItemId.Legendary_Sigil_of_Demons;
		code.WeaponSet2.MainHand  = WeaponType.Staff;
		code.WeaponSet2.Sigil2 = ItemId.Legendary_Sigil_of_Concentration;

		const effective = ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);

		expect(effective.MainHand).toBe(WeaponType.Dagger);
		expect(effective.Sigil1).toBe(ItemId.Legendary_Sigil_of_Demons);
		expect(effective.OffHand).toBe(WeaponType._UNDEFINED);
		expect(effective.Sigil2).toBe(ItemId._UNDEFINED);


		const reference = [ SkillId.Necrotic_Slash, SkillId.Life_Siphon, SkillId.Dark_Pact, SkillId._UNDEFINED, SkillId._UNDEFINED ];

		for(let i = 0; i < reference.length; i++)
			expect(await APICache.ResolveWeaponSkill(code, effective, i)).toBe(reference[i]);
	});

	test('ResolveThiefWeaponSkillsDD', async () => {
		var code = new BuildCode();
		code.Profession = Profession.Thief;
		code.WeaponSet1.MainHand = WeaponType.Dagger;
		code.WeaponSet1.OffHand  = WeaponType.Dagger;

		var reference = [ SkillId.Double_Strike, SkillId.Heartseeker, SkillId.Death_Blossom, SkillId.Dancing_Dagger, SkillId.Cloak_and_Dagger ];
		for(let i = 0; i < reference.length; i++)
			expect(await APICache.ResolveWeaponSkill(code, code.WeaponSet1, i)).toBe(reference[i]);
	});

	test('ResolveThiefWeaponSkillsNoSecond', async () => {
		var code = new BuildCode();
		code.Profession = Profession.Thief;
		code.WeaponSet1.MainHand = WeaponType.Dagger;
		code.WeaponSet1.OffHand  = WeaponType._UNDEFINED;

		var reference = [ SkillId.Double_Strike, SkillId.Heartseeker, SkillId.Twisting_Fangs, SkillId._UNDEFINED, SkillId._UNDEFINED ];
		for(let i = 0; i < reference.length; i++)
			expect(await APICache.ResolveWeaponSkill(code, code.WeaponSet1, i)).toBe(reference[i]);
	});

	test('ResolveTraitId', async () => {
		const code = new BuildCode();
		code.Profession = Profession.Mesmer;
		code.Specializations.Choice1.SpecializationId = SpecializationId.Dueling;
		code.Specializations.Choice1.Choices.Adept    = TraitLineChoice.MIDDLE;

		const id = await APICache.ResolveTrait(code.Specializations.Choice1, TraitSlot.Adept);

		expect(id).toBe(TraitId.Desperate_Decoy);
	});

	/* regression: the third skill on ele staff was not the correct attunement */
	test('LoadEleStaffSkills', async () => {
		var code = new BuildCode();
		code.Profession = Profession.Elementalist;
		code.WeaponSet1.MainHand = WeaponType.Staff;

		var weapons = ResolveEffectiveWeapons(code, WeaponSetNumber.Set1);
		var thirdSkill = await APICache.ResolveWeaponSkill(code, weapons, 2);

		expect(thirdSkill).toBe(SkillId.Flame_Burst);
	});
});
