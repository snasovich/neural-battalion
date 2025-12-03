# GitHub Actions CI/CD Setup

This directory contains the GitHub Actions workflows for the Neural Battalion project.

## Workflows

### Build Workflow (`build.yml`)

This workflow ensures that the Unity project compiles successfully before merging pull requests.

**Triggers:**
- Push to `main` branch
- Pull requests targeting `main` branch
- Manual trigger via workflow_dispatch

**What it does:**
1. Checks out the repository code
2. Caches the Unity Library folder for faster subsequent builds
3. Builds the Unity project using GameCI Unity Builder
4. Uploads the build artifacts for verification

## Setup Instructions

To enable the build workflow, you need to set up Unity activation secrets in your GitHub repository:

### Option 1: Personal License (Recommended for Open Source)

1. Request a Unity activation file:
   ```bash
   # Run this workflow manually from the Actions tab
   # It will generate an activation file
   ```

2. Activate Unity manually at: https://license.unity3d.com/manual

3. Add the following secrets to your GitHub repository:
   - `UNITY_LICENSE`: The content of your `.ulf` license file
   - `UNITY_EMAIL`: Your Unity account email
   - `UNITY_PASSWORD`: Your Unity account password

### Option 2: Professional License

If you have a Unity Pro/Plus license:

1. Go to your repository Settings → Secrets and variables → Actions
2. Add the following repository secrets:
   - `UNITY_LICENSE`: Your Unity license content
   - `UNITY_EMAIL`: Your Unity account email
   - `UNITY_PASSWORD`: Your Unity account password

### Alternative: Free Activation Workflow

If you prefer to use the free personal license workflow:

1. Create a new workflow run for activation
2. Follow the GameCI documentation: https://game.ci/docs/github/activation

## Build Status

Once set up, the build status will appear on pull requests, preventing merge of PRs that break compilation.

## Troubleshooting

### Build fails with "No valid Unity license"
- Ensure all three secrets (UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD) are set correctly
- Verify your Unity license is valid and not expired

### Cache issues
- If builds are slow, check that the Library cache is being created and restored
- You can manually clear the cache from the Actions tab if needed

### Version mismatch
- Ensure the `unityVersion` in build.yml matches the version in `ProjectSettings/ProjectVersion.txt`
- Current version: 2022.3.0f1

## Resources

- [GameCI Documentation](https://game.ci/docs)
- [Unity Builder Action](https://github.com/game-ci/unity-builder)
- [Unity License Activation](https://game.ci/docs/github/activation)
