using UnityEngine;

namespace Assets
{
    public class HealthBar : Billboard
    {
        [System.NonSerialized]
        public bool ShowBar = true;

        public Canvas Canvas;
        public RectTransform HealthImage;
        public RectTransform HungerImage;

        private Specimen specimen;

        private bool _LastShowBar;
        private float _LastHealth;
        private float _LastHunger;

        public override void Start()
        {
            base.Start();

            var specimenBehaviour = transform.parent.GetComponent<SpecimenBehaviour>();
            Canvas.transform.localPosition = new Vector3(0, 1.8f, 0);
            Canvas.enabled = _LastShowBar;
            Canvas.worldCamera = Camera.main;

            specimen = Database.Instance.Specimen.Objects[specimenBehaviour.Id];
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (specimen == null)
                return;

            if (HealthImage && specimen.health != _LastHealth)
            {
                var scale = HealthImage.localScale;
                scale.x = specimen.health * 1.0f / specimen.maxHealth;
                if (scale.x < 0) scale.x = 0;
                HealthImage.localScale = scale;
                _LastHealth = specimen.health;
            }
            if (HungerImage && specimen.hunger != _LastHunger)
            {
                var scale = HungerImage.localScale;
                scale.x = specimen.hunger * 1.0f / specimen.maxHunger;
                if (scale.x < 0) scale.x = 0;
                HungerImage.localScale = scale;
                _LastHunger = specimen.hunger;
            }
            if (_LastShowBar != ShowBar)
            {
                Canvas.enabled = ShowBar;
                _LastShowBar = ShowBar;
            }

        }
    }
}
