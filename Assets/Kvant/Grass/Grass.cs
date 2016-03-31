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
        float _density = 100;

        public float density {
            get { return _density; }
        }

        [SerializeField]
        Vector2 _extent = new Vector2(10, 10);

        public Vector2 extent {
            get { return _extent; }
            set { _extent = value; _positionUpdateFlag = true; }
        }

        [SerializeField]
        Vector2 _offset;

        public Vector2 offset {
            get { return _offset; }
            set { _offset = value; _positionUpdateFlag = true; }
        }

        #endregion

        #region Rotation Parameters

        [SerializeField, Range(0, 90)]
        float _randomPitchAngle = 45;

        public float randomPitchAngle {
            get { return _randomPitchAngle; }
            set { _randomPitchAngle = value; }
        }

        [SerializeField, Range(0, 90)]
        float _noisePitchAngle = 30.0f;

        public float noisePitchAngle {
            get { return _noisePitchAngle; }
            set { _noisePitchAngle = value; }
        }

        [SerializeField]
        float _rotationNoiseFrequency = 1.0f;

        public float rotationNoiseFrequency {
            get { return _rotationNoiseFrequency; }
            set { _rotationNoiseFrequency = value; }
        }

        [SerializeField]
        float _rotationNoiseSpeed = 0.5f;

        public float rotationNoiseSpeed {
            get { return _rotationNoiseSpeed; }
            set { _rotationNoiseSpeed = value; }
        }

        [SerializeField]
        Vector3 _rotationNoiseAxis = Vector3.right;

        public Vector3 rotationNoiseAxis {
            get { return _rotationNoiseAxis; }
            set { _rotationNoiseAxis = value; }
        }

        #endregion

        #region Scale Parameters

        [SerializeField]
        Vector3 _baseScale = Vector3.one;

        public Vector3 baseScale {
            get { return _baseScale; }
            set { _baseScale = value; _scaleUpdateFlag = true; }
        }

        [SerializeField]
        float _minRandomScale = 0.8f;

        public float minRandomScale {
            get { return _minRandomScale; }
            set { _minRandomScale = value; _scaleUpdateFlag = true; }
        }

        [SerializeField]
        float _maxRandomScale = 1.0f;

        public float maxRandomScale {
            get { return _maxRandomScale; }
            set { _maxRandomScale = value; _scaleUpdateFlag = true; }
        }

        [SerializeField]
        float _scaleNoiseAmplitude = 0.5f;

        public float scaleNoiseAmplitude {
            get { return _scaleNoiseAmplitude; }
            set { _scaleNoiseAmplitude = value; _scaleUpdateFlag = true; }
        }

        [SerializeField]
        float _scaleNoiseFrequency = 0.5f;

        public float scaleNoiseFrequency {
            get { return _scaleNoiseFrequency; }
            set { _scaleNoiseFrequency = value; _scaleUpdateFlag = true; }
        }

        #endregion

        #region Render Settings

        [SerializeField]
        Mesh[] _shapes;

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

        #region Built-in Resources

        [SerializeField] Material _defaultMaterial;
        [SerializeField] Shader _kernelShader;

        #endregion

        #region Private Variables And Properties

        RenderTexture _positionBuffer;
        RenderTexture _rotationBuffer;
        RenderTexture _scaleBuffer;

        BulkMesh _bulkMesh;
        Material _kernelMaterial;

        float _rotationNoiseTime;

        bool _positionUpdateFlag;
        bool _scaleUpdateFlag;
        bool _needsReset = true;

        public int InstancePerDraw {
            get { return 4096; }
        }

        public int DrawCount {
            get {
                var c = _density * _extent.x * _extent.y / InstancePerDraw;
                return Mathf.Max(1, Mathf.CeilToInt(c));
            }
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
            m.SetVector("_Scroll", new Vector2(_offset.x / _extent.x, _offset.y / _extent.y));

            m.SetFloat("_RandomPitch", _randomPitchAngle * Mathf.Deg2Rad);
            m.SetVector("_RotationNoise", new Vector3(_rotationNoiseFrequency, _noisePitchAngle * Mathf.Deg2Rad, _rotationNoiseTime));
            m.SetVector("_RotationAxis", _rotationNoiseAxis.normalized);

            m.SetVector("_BaseScale", _baseScale);
            m.SetVector("_RandomScale", new Vector2(_minRandomScale, _maxRandomScale));
            m.SetVector("_ScaleNoise", new Vector2(_scaleNoiseFrequency, _scaleNoiseAmplitude));
        }

        void ResetResources()
        {
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(_shapes, InstancePerDraw);
            else
                _bulkMesh.Rebuild(_shapes, InstancePerDraw);

            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_rotationBuffer) DestroyImmediate(_rotationBuffer);
            if (_scaleBuffer) DestroyImmediate(_scaleBuffer);

            _positionBuffer = CreateBuffer();
            _rotationBuffer = CreateBuffer();
            _scaleBuffer = CreateBuffer();

            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);
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

            // Advance the time variables.
            _rotationNoiseTime += _rotationNoiseSpeed * Time.deltaTime;

            // Call the kernels.
            UpdateKernelShader();

            if (_needsReset || _positionUpdateFlag)
                Graphics.Blit(null, _positionBuffer, _kernelMaterial, 0);

            Graphics.Blit(null, _rotationBuffer, _kernelMaterial, 1);

            if (_needsReset || _scaleUpdateFlag)
                Graphics.Blit(null, _scaleBuffer,    _kernelMaterial, 2);

            // Make a material property block for the following drawcalls.
            var props = new MaterialPropertyBlock();
            props.SetTexture("_PositionTex", _positionBuffer);
            props.SetTexture("_RotationTex", _rotationBuffer);
            props.SetTexture("_ScaleTex", _scaleBuffer);

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
                props.SetVector("_BufferOffset", uv);
                Graphics.DrawMesh(
                    mesh, position, rotation,
                    material, 0, null, 0, props,
                    _castShadows, _receiveShadows);
            }

            // Clear flag variables.
            _positionUpdateFlag = true;
            _scaleUpdateFlag = true;
            _needsReset = false;
        }

        #endregion
    }
}
