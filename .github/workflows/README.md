# GitHub Actions CI/CD Setup

This directory contains the GitHub Actions workflows for the Neural Battalion project.

## Workflows

### Code Quality Workflow (`build.yml`)

This workflow performs C# code quality checks without requiring a Unity license. It validates that C# scripts have proper syntax and follow project conventions.

**Triggers:**
- Push to `main` branch
- Pull requests targeting `main` branch
- Manual trigger via workflow_dispatch

**What it does:**
1. Checks out the repository code
2. Sets up .NET for C# validation tools
3. Validates C# file syntax using Roslyn
4. Checks for common code quality issues:
   - Proper namespace usage (NeuralBattalion.*)
   - TODO/FIXME comments
   - File formatting (line endings, trailing whitespace)
5. Provides summary and local testing instructions

**Note:** This workflow does **NOT** require Unity license or secrets. It performs basic validation to catch syntax errors and common issues before full Unity compilation.

### Activation Workflow (`activation.yml`)

Helper workflow to generate Unity activation files if you want to add full Unity compilation in the future.

## Setup Instructions

### Current Setup (No Unity License Required)

The code quality workflow runs automatically with no additional setup required. It will:
- ✓ Catch C# syntax errors
- ✓ Validate project structure
- ✓ Check code quality and conventions
- ✗ Does NOT perform full Unity compilation (requires Unity license)

### Optional: Add Full Unity Compilation

If you want to add full Unity build checks in the future:

1. Request a Unity activation file:
   - Run the `activation.yml` workflow manually from the Actions tab
   - Download the generated activation file

2. Activate Unity manually at: https://license.unity3d.com/manual

3. Add the following secrets to your GitHub repository:
   - `UNITY_LICENSE`: The content of your `.ulf` license file
   - `UNITY_EMAIL`: Your Unity account email
   - `UNITY_PASSWORD`: Your Unity account password

4. Modify `build.yml` to use GameCI Unity Builder (see GameCI documentation)

## Build Status

The code quality checks will appear on pull requests, helping catch common issues before merging.

## Troubleshooting

### Workflow fails on syntax check
- Review the error message to identify which C# file has syntax errors
- Fix the syntax errors in your local Unity Editor
- Commit and push the fixes

### False positives on validation
- The workflow uses basic syntax checking and may not catch all Unity-specific errors
- Always test your code in Unity Editor before pushing

## Resources

- [GameCI Documentation](https://game.ci/docs)
- [Unity Builder Action](https://github.com/game-ci/unity-builder)
- [Unity License Activation](https://game.ci/docs/github/activation)
