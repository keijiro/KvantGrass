//
// Grass - grassland renderer
//
using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode]
    [AddComponentMenu("Kvant/Grass")]
    public partial class Grass : MonoBehaviour
    {
        #region Basic Properties

        [SerializeField]
        float _density = 50;

        public float density {
            get { return _density; }
        }

        [SerializeField]
        Vector2 _extent = new Vector2(20, 20);

        public Vector2 extent {
            get { return _extent; }
            set { _extent = value; }
        }

        [SerializeField]
        float _randomPitchAngle = 45;

        #endregion

        #region Render Settings

        [SerializeField]
        Vector3 _baseScale = Vector3.one;

        public Vector3 baseScale {
            get { return _baseScale; }
            set { _baseScale = value; }
        }

        [SerializeField]
        float _minRandomScale = 0.8f;

        public float minRandomScale {
            get { return _minRandomScale; }
            set { _minRandomScale = value; }
        }

        [SerializeField]
        float _maxRandomScale = 1.0f;

        public float maxRandomScale {
            get { return _maxRandomScale; }
            set { _maxRandomScale = value; }
        }

        [SerializeField]
        Material _material;
        bool _owningMaterial; // whether owning the material

        public Material sharedMaterial {
            get { return _material; }
            set { _material = value; }
        }

        public Material material {
            get {
                if (!_owningMaterial) {
                    _material = Instantiate<Material>(_material);
                    _owningMaterial = true;
                }
                return _material;
            }
            set {
                if (_owningMaterial) Destroy(_material, 0.1f);
                _material = value;
                _owningMaterial = false;
            }
        }

        [SerializeField]
        ShadowCastingMode _castShadows;

        public ShadowCastingMode castShadows {
            get { return _castShadows; }
            set { _castShadows = value; }
        }

        [SerializeField]
        bool _receiveShadows = false;

        public bool receiveShadows {
            get { return _receiveShadows; }
            set { _receiveShadows = value; }
        }

        #endregion

        #region Editor Properties

        [SerializeField]
        bool _debug;

        #endregion

        #region Built-in Resources

        [SerializeField] Mesh[] _defaultShapes;
        [SerializeField] Material _defaultMaterial;
        [SerializeField] Shader _kernelShader;

        #endregion

        #region Private Variables And Properties

        RenderTexture _positionBuffer;
        RenderTexture _rotationBuffer;
        RenderTexture _scaleBuffer;
        BulkMesh _bulkMesh;
        Material _kernelMaterial;
        bool _needsReset = true;

        public int InstancePerDraw {
            get { return 4096; }
        }

        public int DrawCount {
            get { return 40; }
        }

        #endregion

        #region Resource Management

        public void NotifyConfigChange()
        {
            _needsReset = true;
        }

        Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        RenderTexture CreateBuffer()
        {
            var buffer = new RenderTexture(InstancePerDraw, DrawCount, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        void UpdateKernelShader()
        {
            var m = _kernelMaterial;
            m.SetVector("_Extent", _extent);
            m.SetFloat("_RandomPitch", _randomPitchAngle * Mathf.Deg2Rad * 2);
            m.SetVector("_BaseScale", _baseScale);
            m.SetVector("_RandomScale", new Vector2(_minRandomScale, _maxRandomScale));
        }

        void ResetResources()
        {
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(_defaultShapes, InstancePerDraw);
            else
                _bulkMesh.Rebuild(_defaultShapes, InstancePerDraw);

            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_rotationBuffer) DestroyImmediate(_rotationBuffer);
            if (_scaleBuffer) DestroyImmediate(_scaleBuffer);

            _positionBuffer = CreateBuffer();
            _rotationBuffer = CreateBuffer();
            _scaleBuffer = CreateBuffer();

            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);

            _needsReset = false;
        }

        #endregion

        #region MonoBehaviour Functions

        void Reset()
        {
            _needsReset = true;
        }

        void OnDestroy()
        {
            if (_bulkMesh != null) _bulkMesh.Release();
            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_rotationBuffer) DestroyImmediate(_rotationBuffer);
            if (_scaleBuffer)    DestroyImmediate(_scaleBuffer);
            if (_kernelMaterial) DestroyImmediate(_kernelMaterial);
        }

        void Update()
        {
            if (_needsReset) ResetResources();

            // Call the kernels.
            UpdateKernelShader();
            Graphics.Blit(null, _positionBuffer, _kernelMaterial, 0);
            Graphics.Blit(null, _rotationBuffer, _kernelMaterial, 1);
            Graphics.Blit(null, _scaleBuffer,    _kernelMaterial, 2);

            // Make a material property block for the following drawcalls.
            var props = new MaterialPropertyBlock();
            props.AddTexture("_PositionTex", _positionBuffer);
            props.AddTexture("_RotationTex", _rotationBuffer);
            props.AddTexture("_ScaleTex", _scaleBuffer);

            // Temporary variables.
            var mesh = _bulkMesh.mesh;
            var position = transform.position;
            var rotation = transform.rotation;
            var material = _material ? _material : _defaultMaterial;
            var uv = new Vector2(0.5f / _positionBuffer.width, 0);

            // Draw mesh segments.
            for (var i = 0; i < _positionBuffer.height; i++)
            {
                uv.y = (0.5f + i) / _positionBuffer.height;
                props.AddVector("_BufferOffset", uv);
                Graphics.DrawMesh(
                    mesh, position, rotation,
                    material, 0, null, 0, props,
                    _castShadows, _receiveShadows);
            }
        }

        #endregion
    }
}
