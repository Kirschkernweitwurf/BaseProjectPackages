using System.Collections;
using System.Collections.Generic;
using Base.CorePackage.CameraUtility;
using Base.UIPackage.Utility;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.UIPackage.PlayTests
{
    /// <summary>
    /// The first coverage this package has. The billboard resolves the camera in <c>Awake</c> and does
    /// its work in <c>LateUpdate</c>, so both the service lookup and the rotation only happen once
    /// frames are running.
    /// </summary>
    public sealed class BillboardPlayTests
    {
        private const float CameraHeight = 3f;
        private const float CameraDistance = 10f;
        private const float ToleranceDegrees = 0.01f;

        private readonly List<GameObject> _hosts = new();

        private CameraProvider _provider;

        /// <summary>
        /// Puts a camera in the scene and hands it to the provider directly, so the test does not
        /// depend on which object happens to carry the MainCamera tag in the test scene.
        /// </summary>
        [UnitySetUp]
        public IEnumerator Prepare()
        {
            _provider = CreateHost(nameof(CameraProvider)).AddComponent<CameraProvider>();

            Camera camera = CreateHost(nameof(Camera)).AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, CameraHeight, -CameraDistance);
            camera.transform.LookAt(Vector3.zero);

            _provider.SetMainCamera(camera);

            yield return null;
        }

        /// <summary>Hands back everything the test put in the scene, including the service entry.</summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            _hosts.Clear();
            _provider = null;
        }

        /// <summary>
        /// With the Y axis unlocked the billboard copies the camera's facing outright, so a quad using
        /// it stays parallel to the screen rather than merely pointed at the camera.
        /// </summary>
        [UnityTest]
        public IEnumerator ABillboardCopiesTheCameraFacing()
        {
            Billboard billboard = CreateHost(nameof(Billboard)).AddComponent<Billboard>();
            billboard.transform.rotation = Quaternion.Euler(Vector3.one);

            yield return null;

            _provider.TryGetMainTransform(out Transform cameraTransform);
            float offset = Vector3.Angle(billboard.transform.forward, cameraTransform.forward);

            Assert.That(offset, Is.LessThan(ToleranceDegrees));
        }

        /// <summary>
        /// A billboard keeps following the camera rather than facing it once and stopping, so moving
        /// the camera has to move it again on the next frame.
        /// </summary>
        [UnityTest]
        public IEnumerator ABillboardFollowsTheCameraWhenItMoves()
        {
            Billboard billboard = CreateHost(nameof(Billboard)).AddComponent<Billboard>();

            yield return null;

            _provider.TryGetMainTransform(out Transform cameraTransform);
            cameraTransform.position = new Vector3(CameraDistance, CameraHeight, 0f);
            cameraTransform.LookAt(Vector3.zero);

            yield return null;

            float offset = Vector3.Angle(billboard.transform.forward, cameraTransform.forward);

            Assert.That(offset, Is.LessThan(ToleranceDegrees));
        }

        /// <summary>Creates an object and remembers it so the teardown can clean it up.</summary>
        private GameObject CreateHost(string name)
        {
            GameObject host = new(name);
            _hosts.Add(host);

            return host;
        }
    }
}