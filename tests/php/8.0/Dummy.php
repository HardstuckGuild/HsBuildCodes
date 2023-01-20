<?php namespace PHPUnit\Framework;

/** NOTE(Rennorb): This file and class definition exists solely to allow for type hinting
 *  the TestClass interface since most typecheckers can't inspect the phar file. */
class TestCase {
	public function assertEquals(mixed $expected, mixed $actual) : void { }
	public function assertNotEquals(mixed $expected, mixed $actual) : void { }
	public function assertNull(mixed $actual) : void { }
	public function assertTrue(bool $actual) : void { }
	public function assertFalse(bool $actual) : void { }
	public function assertInstanceOf(string $className, mixed $actual) : void { }
	public function assertGreaterThan(int $min, mixed $actual) : void { }
	public function assertContains(mixed $needle, mixed $haystack) : void { }
	
	public function expectException(string $className) : void { }
	public function expectErrorMessage(string $message) : void { }
	public function expectErrorMessageMatches(string $regex) : void { }
	
	public function expectWarning() : void { }
	public function expectWarningMessage(string $message) : void { }
	public function expectWarningMessageMatches(string $regex) : void { }

	public function markTestSkipped(string $reason): void { }
}