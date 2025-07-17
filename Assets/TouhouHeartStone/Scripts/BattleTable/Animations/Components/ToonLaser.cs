using UnityEngine;

namespace Game.Animations
{
    public class ToonLaser : MonoBehaviour
    {
        public void SetStartPoint(Vector3 position)
        {
            if (laserMat == null)
                return;
            position += launchOffset;
            transform.position = position;
            laserMat.SetVector("_StartPoint", position);
        }
        public void SetEndPoint(Vector3 position)
        {
            if (laserMat == null || laserPS == null || _startDissovle)
                return;
            position += hitOffset;
            float distance = Vector3.Distance(transform.position, position);
            Vector3 normal = (position - transform.position).normalized;
            transform.forward = normal;
            particleCount = Mathf.RoundToInt(distance / (2 * laserScale));
            if (particleCount < distance / (2 * laserScale))
            {
                particleCount += 1;
            }
            particlesPositions = new Vector3[particleCount];
            AddParticles();

            laserMat.SetFloat("_Distance", distance);
            laserMat.SetVector("_EndPoint", position);
            if (Hit != null)
            {
                hitEffect.transform.position = position + normal * hitDirectionOffset;
                hitEffect.transform.LookAt(position);
                foreach (var AllHits in Hit)
                {
                    if (!AllHits.isPlaying) AllHits.Play();
                }
                foreach (var AllFlashes in Flash)
                {
                    if (!AllFlashes.isPlaying) AllFlashes.Play();
                }
            }
        }
        public void DisablePrepare()
        {
            transform.parent = null;
            dissovleTimer = 0;
            _startDissovle = true;
            if (Flash != null && Hit != null)
            {
                foreach (var AllHits in Hit)
                {
                    if (AllHits.isPlaying) AllHits.Stop();
                }
                foreach (var AllFlashes in Flash)
                {
                    if (AllFlashes.isPlaying) AllFlashes.Stop();
                }
            }
        }
        protected void Awake()
        {
            laserPS = GetComponent<ParticleSystem>();
            laserMat = GetComponent<ParticleSystemRenderer>().material;
            Flash = flashEffect.GetComponentsInChildren<ParticleSystem>();
            Hit = hitEffect.GetComponentsInChildren<ParticleSystem>();
            laserMat.SetFloat("_Scale", laserScale);
        }
        protected void Update()
        {
            if (_startDissovle)
            {
                dissovleTimer += Time.deltaTime;
                laserMat.SetFloat("_Dissolve", dissovleTimer * 5);
            }
        }
        private void AddParticles()
        {
            particles = new ParticleSystem.Particle[particleCount];

            for (int i = 0; i < particleCount; i++)
            {
                particlesPositions[i] = new Vector3(0f, 0f, 0f) + new Vector3(0f, 0f, i * 2 * laserScale);
                particles[i].position = particlesPositions[i];
                particles[i].startSize3D = new Vector3(0.001f, 0.001f, 2 * laserScale);
                particles[i].startColor = laserColor;
            }
            laserPS.SetParticles(particles, particles.Length);
        }

        public bool startDissovle => _startDissovle;
        public float laserScale = 1;
        public Color laserColor = new Vector4(1, 1, 1, 1);
        public GameObject hitEffect;
        public GameObject flashEffect;
        public Vector3 launchOffset;
        public Vector3 hitOffset;
        public float hitDirectionOffset = 0;
        public float maxLength;

        private ParticleSystem laserPS;
        private ParticleSystem[] Flash;
        private ParticleSystem[] Hit;
        private Material laserMat;
        private int particleCount;
        private ParticleSystem.Particle[] particles;
        private Vector3[] particlesPositions;
        private float dissovleTimer = 0;
        private bool _startDissovle = false;
    }
}