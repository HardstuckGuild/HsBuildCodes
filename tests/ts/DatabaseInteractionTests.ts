import { describe, expect, test } from "@jest/globals";
import PerProfessionData from "../../include/ts/Database/PerProfessionData";
import { Profession } from "../../include/ts/Structures";

describe('DatabaseInteractionTests', () => {
	test('CanDownloadSkillPallettes', () => {
		PerProfessionData.Reload(Profession.Revenant);
		expect(PerProfessionData.Revenant.SkillToPallette.length).toBeGreaterThan(2);
	});

	// test('CanFindOfflinePallette', () => {
	// 	PerProfessionData.Reload(Profession.Necromancer, true);
	// 	expect(PerProfessionData.Necromancer.SkillToPallette.length).toBeGreaterThan(2);
	// });
});
