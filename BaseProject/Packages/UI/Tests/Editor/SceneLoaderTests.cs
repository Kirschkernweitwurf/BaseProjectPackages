using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Base.UIPackage.Utility;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.UIPackage.Tests
{
    /// <summary>
    /// The two ways a scene load is refused before it starts. Every button in this package that
    /// changes scenes goes through here, and both refusals happen before anything is unloaded, so a
    /// misconfigured button leaves the running scene where it is rather than tearing it down first.
    /// </summary>
    /// <remarks>
    /// The load itself needs a scene loading manager and a build with the scene in it, so what is
    /// reachable here is the pair of guards in front of it. Both return before the first await, which
    /// is why the task is already finished when it comes back.
    /// </remarks>
    public sealed class SceneLoaderTests
    {
        private const string MissingManagerMessage = "not registered";
        private const string MissingSceneMessage = "No scene name";
        private const string SceneName = "MainMenu";

        /// <summary>
        /// A button whose scene name was never filled in is a wiring mistake, so it is reported rather
        /// than unloading the running scene and landing on nothing.
        /// </summary>
        /// <param name="sceneName">The scene name the button carries.</param>
        [TestCase(null)]
        [TestCase("")]
        public void LoadingWithoutASceneNameIsRefusedAndReported(string sceneName)
        {
            LogAssert.Expect(LogType.Error, new Regex(MissingSceneMessage));

            Task task = SceneLoader.LoadSceneAsync(sceneName, null);

            Assert.That(task.IsCompleted, Is.True);
        }

        /// <summary>
        /// Without a scene loading manager there is nothing to load through. The lookup reports that
        /// itself, and the refusal happens before the running scene is touched.
        /// </summary>
        [Test]
        public void LoadingWithoutAManagerIsRefusedAndReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(MissingManagerMessage));

            Task task = SceneLoader.LoadSceneAsync(SceneName, null);

            Assert.That(task.IsCompleted, Is.True);
        }
    }
}