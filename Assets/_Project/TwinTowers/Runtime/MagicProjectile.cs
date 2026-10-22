using UnityEngine;

namespace KingdomBorder.TwinTowers
{
    public sealed class MagicProjectile : MonoBehaviour
    {
        public SpriteRenderer body;
        public Sprite haloSprite;
        public Material trailMaterial;
        private EnemyAgent target;
        private float damage, penetration, slow, duration, speed, age;
        private int source;
        private bool launched;
        private SpriteRenderer halo;
        private readonly SpriteRenderer[] orbiters = new SpriteRenderer[2];
        public void Launch(EnemyAgent enemy, TowerDefinition settings, int sourceId)
        {
            target = enemy;
            damage = settings.damage;
            penetration = settings.armorPenetration;
            slow = settings.slowFraction;
            duration = settings.slowDuration;
            speed = Mathf.Max(0.1f, settings.projectileSpeed);
            source = sourceId;
            age = 0;
            launched = true;
            if (body) body.color = settings.magicColor;
            if (body && haloSprite)
            {
                halo=ChildSprite("OrbitingRune",haloSprite,new Color(settings.magicColor.r,settings.magicColor.g,settings.magicColor.b,0.55f));
                halo.transform.localScale=Vector3.one*0.8f;
                for(int i=0;i<orbiters.Length;i++)
                {
                    orbiters[i]=ChildSprite("OrbitLight",body.sprite,i==0?Color.white:new Color(0.7f,0.8f,1));
                    orbiters[i].transform.localScale=Vector3.one*0.19f;
                }
                ArcaneBurst.Spawn(transform.position,settings.magicColor,haloSprite,body.sprite,0.42f);
            }
            if (trailMaterial)
            {
                var trail=gameObject.AddComponent<TrailRenderer>();trail.sharedMaterial=trailMaterial;
                trail.time=0.24f;trail.startWidth=0.22f;trail.endWidth=0.01f;trail.minVertexDistance=0.035f;
                trail.sortingOrder=14;trail.numCapVertices=2;trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                var gradient=new Gradient();
                gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(settings.magicColor,0.25f),new GradientColorKey(new Color(0.35f,0.2f,0.9f),1)},
                    new[]{new GradientAlphaKey(0.9f,0),new GradientAlphaKey(0.55f,0.4f),new GradientAlphaKey(0,1)});
                trail.colorGradient=gradient;
            }
        }
        private SpriteRenderer ChildSprite(string name,Sprite sprite,Color color)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.color=color;sr.sortingOrder=16;return sr;
        }
        private void Update()
        {
            if (!launched || !target || !target.IsAlive) { Destroy(gameObject); return; }
            age += Time.deltaTime;
            if(halo)
            {
                halo.transform.localRotation=Quaternion.Euler(0,0,age*360);
                for(int i=0;i<orbiters.Length;i++)
                {
                    float a=age*12+i*Mathf.PI;
                    orbiters[i].transform.localPosition=new Vector3(Mathf.Cos(a),Mathf.Sin(a))*0.25f;
                }
            }
            if (age > 8) { Destroy(gameObject); return; }
            Vector3 aim = target.transform.position + Vector3.up * 0.25f;
            transform.position = Vector3.MoveTowards(transform.position, aim, speed * Time.deltaTime);
            if ((transform.position - aim).sqrMagnitude > 0.04f) return;
            target.TakeDamage(damage, penetration);
            if (target && target.IsAlive) target.ApplySlow(slow, duration, source);
            if (body && haloSprite) ArcaneBurst.Spawn(aim,body.color,haloSprite,body.sprite);
            Destroy(gameObject);
        }
    }
}
