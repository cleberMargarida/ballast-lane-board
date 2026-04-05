import { cp, mkdir, readdir, rm } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const currentDir = path.dirname(fileURLToPath(import.meta.url));
const clientRoot = path.resolve(currentDir, '..');
const sourceRoot = path.join(clientRoot, 'dist', 'ballast-lane-board', 'browser');
const targetRoot = path.resolve(clientRoot, '..', 'BallastLaneBoard.WebApi', 'wwwroot');
const preservedFiles = new Set(['.gitignore', 'swagger-dark.css']);

async function syncSpaAssets() {
  const sourceEntries = await readdir(sourceRoot, { withFileTypes: true });

  await mkdir(targetRoot, { recursive: true });

  const targetEntries = await readdir(targetRoot, { withFileTypes: true });
  for (const entry of targetEntries) {
    if (preservedFiles.has(entry.name)) {
      continue;
    }

    await rm(path.join(targetRoot, entry.name), { force: true, recursive: true });
  }

  for (const entry of sourceEntries) {
    await cp(path.join(sourceRoot, entry.name), path.join(targetRoot, entry.name), {
      force: true,
      recursive: true,
    });
  }

  console.log(`Synced SPA assets from ${sourceRoot} to ${targetRoot}.`);
}

syncSpaAssets().catch(error => {
  console.error('Failed to sync SPA assets to the WebApi wwwroot.', error);
  process.exitCode = 1;
});