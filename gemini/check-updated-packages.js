import { spawnSync } from "node:child_process";
import { readFileSync, writeFileSync } from "node:fs";

const pkgJsonString = readFileSync('./package.json', 'utf-8');

const isUpgrade = process.argv.includes('--up');

const pkgJson = JSON.parse(pkgJsonString);

const dependencies = pkgJson.dependencies || {};
const devDependencies = pkgJson.devDependencies || {};

function checkUpdatedPackages(deps, isDevDeps) {
    const updatedPackages = [];

    for (const [pkg, version] of Object.entries(deps)) {
        console.log(`Checking package ${pkg}@${version}...`);
        const result = spawnSync('npm', ['view', pkg, 'version'], { encoding: 'utf-8' });

        if (result.error) {
            console.error(`Error fetching version for package ${pkg}:`, result.error);
            continue;
        }

        const latestVersion = result.stdout.trim();

        if (latestVersion !== version) {
            console.log(`Package ${pkg} is outdated: ${version} -> ${latestVersion}`);
            updatedPackages.push({ package: pkg, current: version, latest: latestVersion });
            (isDevDeps ? pkgJson.devDependencies : pkgJson.dependencies)[pkg] = latestVersion;
        }
    }

    return updatedPackages;
}

const updatedDeps = checkUpdatedPackages(dependencies, false);
const updatedDevDeps = checkUpdatedPackages(devDependencies, true);

if (updatedDeps.length || updatedDevDeps.length) {
    console.log("Some packages are outdated.");
    if (isUpgrade) {
        const updatedPkgJsonString = JSON.stringify(pkgJson, null, 4);
        console.log("Updating package.json with latest versions...");
        writeFileSync('./package.json', updatedPkgJsonString, 'utf-8');
        console.log("package.json updated.");

        spawnSync('npm', ['install'], { stdio: 'inherit' });
        console.log("Dependencies reinstalled.");
    } else {
        console.log("Run the script with '--up' flag to update package.json with latest versions.");
    }
}