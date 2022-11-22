/** @type {import('jest').Config} */
module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  detectOpenHandles: true,
  testMatch: [ '**/*Tests.ts' ]
};
