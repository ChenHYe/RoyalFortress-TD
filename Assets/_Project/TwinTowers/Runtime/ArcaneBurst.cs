using UnityEngine;

namespace KingdomBorder.TwinTowers
{
    public sealed class ArcaneBurst : MonoBehaviour
    {
        private SpriteRenderer ring, flash;
        private readonly SpriteRenderer[] sparks = new SpriteRenderer[8];
        private Color tint;
        private float age, strength;
        public static void Spawn(Vector3 position, Color color, Sprite halo, Sprite core, float intensity=1)
        {
            var go=new GameObject("ArcaneBurst");go.transform.position=position;
            var burst=go.AddComponent<ArcaneBurst>();burst.tint=color;burst.strength=intensity;
            burst.ring=burst.Make("ExpandingRune",halo,color);
            burst.flash=burst.Make("BrightCore",core,Color.white);
            for(int i=0;i<burst.sparks.Length;i++) burst.sparks[i]=burst.Make("ArcaneSpark",core,color);
        }
        private SpriteRenderer Make(string name,Sprite sprite,Color color)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=color;renderer.sortingOrder=21;return renderer;
        }
        private void Update()
        {
            age+=Time.deltaTime;float t=Mathf.Clamp01(age/0.4f);
            ring.transform.localScale=Vector3.one*Mathf.Lerp(0.25f,1.65f,t)*strength;
            ring.transform.localRotation=Quaternion.Euler(0,0,t*70);
            Color color=tint;color.a=(1-t)*0.9f;ring.color=color;
            flash.transform.localScale=Vector3.one*(1-t)*0.85f*strength;
            flash.color=new Color(1,0.95f,1,1-t);
            for(int i=0;i<sparks.Length;i++)
            {
                float a=i*Mathf.PI*2/sparks.Length;
                sparks[i].transform.localPosition=new Vector3(Mathf.Cos(a),Mathf.Sin(a))*t*0.85f*strength;
                sparks[i].transform.localScale=Vector3.one*(1-t)*0.25f*strength;
                sparks[i].color=color;
            }
            if(t>=1) Destroy(gameObject);
        }
    }
}
