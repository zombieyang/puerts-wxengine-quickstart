using UnityEngine;
using UnityEditor;

namespace WeChat
{
    [InitializeOnLoad]
    class PuertsExportRegister
    {

        static PuertsExportRegister()
        {
            WXHierarchyContext.registerComponentConverter("080", IteratePuertsComponent);
        }

        private static void IteratePuertsComponent(GameObject go, WXEntity obj, WXHierarchyContext context, ExportPreset preset)
        {
            PuertsBeefBallBehaviour[] behaviours = go.GetComponents<PuertsBeefBallBehaviour>();

            if (context.preset.presetKey == "scene" && go.transform.parent == null) {
                obj.components.Add(context.AddComponent(new PuertsBeefBallRootBehaviour(), null));
            }

            if (behaviours.Length != 0)
            {
                foreach (PuertsBeefBallBehaviour behaviour in behaviours)
                {
                    obj.components.Add(context.AddComponent(new PuertsBeefBallBehaviourConverter(behaviour), behaviour));
                }
            }

            /**
             * 下面的Component为adaptor对齐unity，但引擎也有类似Component的组件
             * 需要在WXUnityComponent中对应做处理，增加ref对象指向对应引擎对象
            */

            obj.components.Add(context.AddComponent(new WXUnityComponent(go.transform), go.transform));

            // Particle System
            ParticleSystem particle = go.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                //Debug.Log("addComponentParticleSystem");
                obj.components.Add(context.AddComponent(new WXUnityComponent(particle), particle));
            }

            // Animator
            Animator animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                obj.components.Add(context.AddComponent(new WXUnityComponent(animator), animator));
            }

            // Animation
            Animation animation = go.GetComponent<Animation>();
            if (animation != null)
            {
                obj.components.Add(context.AddComponent(new WXUnityComponent(animation), animation));
            }

            // Renderers
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (renderer is MeshRenderer)
                {
                    obj.components.Add(context.AddComponent(new WXUnityComponent((MeshRenderer)renderer), renderer));
                    // 由于引擎没有MeshFilter组件，这里强制在导出MeshRenderer的时候带上一个MeshFilter
                    obj.components.Add(context.AddComponent(new WXMeshFilter(), renderer));
                }
                else if (renderer is LineRenderer)
                {
                    obj.components.Add(context.AddComponent(new WXUnityComponent((LineRenderer)renderer), renderer));
                }
                else if (renderer is SkinnedMeshRenderer)
                {
                    obj.components.Add(context.AddComponent(new WXUnityComponent((SkinnedMeshRenderer)renderer), renderer));
                    obj.components.Add(context.AddComponent(new WXMeshFilter(), renderer));
                }
            }

            // Camera
            Camera camera = go.GetComponent<Camera>();
            if (camera != null)
            {
                obj.components.Add(context.AddComponent(new WXUnityComponent(camera), camera));
            }

            // Light
            Light light = go.GetComponent<Light>();
            if (light != null)
            {
                obj.components.Add(context.AddComponent(new WXUnityComponent(light), light));
            }

            // Colliders
            Collider[] colliders = go.GetComponents<Collider>();
            if (colliders != null && colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    if (collider is BoxCollider)
                    {
                        //obj.components.Add(context.AddComponent(new WXBoxCollider((BoxCollider)collider), collider));
                        obj.components.Add(context.AddComponent(new WXUnityComponent(collider), collider));
                    }
                    else if (collider is SphereCollider)
                    {
                        //obj.components.Add(context.AddComponent(new WXSphereCollider((SphereCollider)collider), collider));
                        obj.components.Add(context.AddComponent(new WXUnityComponent(collider), collider));
                    }
                    else if (collider is CapsuleCollider)
                    {
                        //obj.components.Add(context.AddComponent(new WXCapsuleCollider((CapsuleCollider)collider), collider));
                        obj.components.Add(context.AddComponent(new WXUnityComponent(collider), collider));
                    }
                    else if (collider is MeshCollider)
                    {
                        //obj.components.Add(context.AddComponent(new WXMeshCollider((MeshCollider)collider), collider));
                        obj.components.Add(context.AddComponent(new WXUnityComponent(collider), collider));
                    }
                }
            }
        }
    }
}