import { describe, expect, test } from "@jest/globals";
import PerProfessionData from "../../../include/ts/es6/Database/PerProfessionData";
import { Profession } from "../../../include/ts/es6/Structures";

describe('DatabaseInteractionTests', () => {
	test('CanDownloadSkillPallettes', async () => {
		await PerProfessionData.Reload(Profession.Revenant);
		expect(PerProfessionData.Revenant.SkillToPallette[0]).not.toBe(undefined);
	});

	// test('CanFindOfflinePallette', () => {
	// 	PerProfessionData.Reload(Profession.Necromancer, true);
	// 	expect(PerProfessionData.Necromancer.SkillToPallette.length).toBeGreaterThan(2);
	// });
});
