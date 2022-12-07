import { ALL_INFUSION_COUNT, Compress, CompressionOptions, HasInfusionSlot } from "../../../include/ts/es6/Database/Static";
import { BuildCode } from "../../../include/ts/es6/Structures";
import TextLoader from "../../../include/ts/es6/TextLoader";
import TestUtilities from "./TestUtilities";

describe('CompressionTests', () => {
	test('DoNothing', () => {
		const text = TestUtilities.CodesV2["uncompressed1"];
		const code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.NONE);
		const text_compressed = TextLoader.WriteBuildCode(code);
		expect(text_compressed).toBe(text);
	});

	test('ReplaceNonStatInfusions', () => {
		const text = TestUtilities.CodesV2["uncompressed1"];
		const code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.REMOVE_NON_STAT_INFUSIONS);
		const text_compressed = TextLoader.WriteBuildCode(code);
		expect(text_compressed).toBe(TestUtilities.CodesV2["compressed1-no-agony-inf"]);
	});

	test('RearangeInfusions', () => {
		const text = TestUtilities.CodesV2["uncompressed1"];
		const code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.REARRANGE_INFUSIONS);

		//NOTE(Rennorb): cant directly compare since the order could be different
		function ExtractInfusions(code : BuildCode) : object
		{
			const infusions = {};
			for(let i = 0; i < ALL_INFUSION_COUNT; i++)
			{
				if(!HasInfusionSlot(code, i)) continue;

				const item = code.Infusions[i];
				infusions[item] = infusions[item] !== undefined ? infusions[item] + 1 : 1;
			}
			return infusions;
		}
		
		const code_reference = TextLoader.LoadBuildCode(TestUtilities.CodesV2["compressed1-rearange-inf"]);

		expect(ExtractInfusions(code)).toStrictEqual(ExtractInfusions(code_reference));
	});

	test('SubstituteInfusions', () => {
		const text = TestUtilities.CodesV2["uncompressed1"];
		const code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.SUBSTITUTE_INFUSIONS);
		const text_compressed = TextLoader.WriteBuildCode(code);
		expect(text_compressed).toBe(TestUtilities.CodesV2["compressed1-subst-inf"]);
	});

	test('All', () => {
		const text = TestUtilities.CodesV2["uncompressed1"];
		const code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.ALL);
		const text_compressed = TextLoader.WriteBuildCode(code);
		expect(text_compressed).toBe(TestUtilities.CodesV2["compressed1-all"]);
	});
});
