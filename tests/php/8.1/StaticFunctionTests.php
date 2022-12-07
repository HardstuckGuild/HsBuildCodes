<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\Static;

use Hardstuck\GuildWars2\BuildCodes\V2\BuildCode;
use Hardstuck\GuildWars2\BuildCodes\V2\CompressionOptions;
use Hardstuck\GuildWars2\BuildCodes\V2\Tests\TestUtilities;
use Hardstuck\GuildWars2\BuildCodes\V2\TextLoader;
use PHPUnit\Framework\TestCase;

use const Hardstuck\GuildWars2\BuildCodes\V2\ALL_INFUSION_COUNT;

use function Hardstuck\GuildWars2\BuildCodes\V2\Compress;
use function Hardstuck\GuildWars2\BuildCodes\V2\HasInfusionSlot;

set_include_path(__DIR__.'/../../../include/common/');

final class StaticFunctionTests extends TestCase
{
	/** @test */
	public function DoNothing() 
	{
		$text = TestUtilities::$CodesV2["uncompressed1"];
		$code = TextLoader::LoadBuildCode($text);
		Compress($code, CompressionOptions::NONE);
		$text_compressed = TextLoader::WriteBuildCode($code);
		$this->assertEquals($text, $text_compressed);
	}

	/** @test */
	public function ReplaceNonStatInfusions()
	{
		$text = TestUtilities::$CodesV2["uncompressed1"];
		$code = TextLoader::LoadBuildCode($text);
		Compress($code, compressionOptions::REMOVE_NON_STAT_INFUSIONS);
		$text_compressed = TextLoader::WriteBuildCode($code);
		$this->assertEquals(TestUtilities::$CodesV2["compressed1-no-agony-inf"], $text_compressed);
	}

	/** @test */
	public function RearangeInfusions()
	{
		$text = TestUtilities::$CodesV2["uncompressed1"];
		$code = TextLoader::LoadBuildCode($text);
		Compress($code, compressionOptions::REARRANGE_INFUSIONS);

		//NOTE(Rennorb): cant directly compare since the order could be different
		function ExtractInfusions(BuildCode $code)
		{
			$infusions = [];
			for($i = 0; $i < ALL_INFUSION_COUNT; $i++)
			{
				if(!HasInfusionSlot($code, $i)) continue;

				$item = $code->Infusions[$i];
				$infusions[$item] = array_key_exists($item, $infusions) ? $infusions[$item] + 1 : 1;
			}
			return $infusions;
		}
		
		$code_reference = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["compressed1-rearange-inf"]);

		$this->assertEquals(ExtractInfusions($code_reference), ExtractInfusions($code));
	}

	/** @test */
	public function SubstituteInfusions()
	{
		$text = TestUtilities::$CodesV2["uncompressed1"];
		$code = TextLoader::LoadBuildCode($text);
		Compress($code, compressionOptions::SUBSTITUTE_INFUSIONS);
		$text_compressed = TextLoader::WriteBuildCode($code);
		$this->assertEquals(TestUtilities::$CodesV2["compressed1-subst-inf"], $text_compressed);
	}

	/** @test */
	public function All()
	{
		$text = TestUtilities::$CodesV2["uncompressed1"];
		$code = TextLoader::LoadBuildCode($text);
		Compress($code, compressionOptions::ALL);
		$text_compressed = TextLoader::WriteBuildCode($code);
		$this->assertEquals(TestUtilities::$CodesV2["compressed1-all"], $text_compressed);
	}
}