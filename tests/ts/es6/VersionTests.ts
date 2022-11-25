import { describe, expect, test } from "@jest/globals";
import Static from "../../../include/ts/es6/Database/Static";
import TextLoader from "../../../include/ts/es6/TextLoader";
import TestUtilities from "./TestUtilities";

describe('VersionTests', () => {
	function DataProvider() {
		const invalidCodes = [
			TestUtilities.CodesInvalid["wrong-version"],
		];
		
		const v1Codes = TestUtilities.CodesV1;
		
		const v2Codes = TestUtilities.CodesV2;

		return [
			...invalidCodes.map((code) => [code, -1]),
			...Object.values(v1Codes).map((code) => [code, 1]),
			...Object.values(v2Codes).map((code) => {
				let version = TextLoader.INVERSE_CHARSET[code.charCodeAt(0)];
				if(version >= 26) version -= 26;
				return [code, version];
			}),
		];
	}

	test.each(DataProvider())('DetermineCodeVersion', (code : string, expectedVersion : number) => {
		expect(Static.DetermineCodeVersion(code)).toBe(expectedVersion);
	});
});
