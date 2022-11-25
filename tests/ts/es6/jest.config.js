/** @type {import('jest').Config} */
module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  detectOpenHandles: true,
  testTimeout: 60 * 1000,
  testMatch: [ '**/*Tests.ts' ]
};
