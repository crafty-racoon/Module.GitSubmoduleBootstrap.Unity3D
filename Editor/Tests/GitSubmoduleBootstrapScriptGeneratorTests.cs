using NUnit.Framework;

namespace CraftyRacoon.GitSubmoduleBootstrap.Editor.Tests
{
    internal sealed class GitSubmoduleBootstrapScriptGeneratorTests
    {
        [Test]
        public void GeneratedScriptRequiresRepositoryRootAndBootstrapsSubmodules()
        {
            string script = GitSubmoduleBootstrapScriptGenerator.BuildScriptContent();
            StringAssert.Contains("chcp 65001 >nul", script);
            StringAssert.Contains("git rev-parse --is-inside-work-tree", script);
            StringAssert.Contains("git rev-parse --show-prefix", script);
            StringAssert.Contains("This script must be located at the Git repository root.", script);
            StringAssert.DoesNotContain("git rev-parse --show-toplevel", script);
            StringAssert.DoesNotContain("cd /d \"%GIT_ROOT%\"", script);
            StringAssert.Contains("git submodule update --init --recursive", script);
        }

        [Test]
        public void GeneratedScriptDoesNotActAsGitStateManager()
        {
            string script = GitSubmoduleBootstrapScriptGenerator.BuildScriptContent();
            StringAssert.DoesNotContain("git diff", script);
            StringAssert.DoesNotContain("git status", script);
            StringAssert.DoesNotContain("git submodule status", script);
            StringAssert.DoesNotContain("UnityLockfile", script);
            StringAssert.DoesNotContain("powershell", script);
        }

        [Test]
        public void GeneratedScriptBlocksWhileGitIndexIsLockedAndPreservesGitDiagnostics()
        {
            string script = GitSubmoduleBootstrapScriptGenerator.BuildScriptContent();
            StringAssert.Contains("git rev-parse --git-path index.lock", script);
            StringAssert.DoesNotContain("git rev-parse --git-path index.lock 2^>nul", script);
            StringAssert.Contains("A Git index lock exists.", script);
            StringAssert.Contains("Wait for clone / checkout / Git LFS to finish", script);
        }

        [Test]
        public void GeneratedScriptTreatsRepositoryWithoutSubmodulesAsSuccess()
        {
            string script = GitSubmoduleBootstrapScriptGenerator.BuildScriptContent();
            StringAssert.Contains("if not exist \".gitmodules\"", script);
            StringAssert.Contains("No submodule initialization is required.", script);
        }

        [Test]
        public void GeneratedScriptIsDeterministicAndUsesCrLf()
        {
            string first = GitSubmoduleBootstrapScriptGenerator.BuildScriptContent();
            string second = GitSubmoduleBootstrapScriptGenerator.BuildScriptContent();
            Assert.That(second, Is.EqualTo(first));
            StringAssert.EndsWith("\r\n", first);
            Assert.That(first.Contains("\n") && !first.Contains("\r\n"), Is.False);
        }
    }
}
