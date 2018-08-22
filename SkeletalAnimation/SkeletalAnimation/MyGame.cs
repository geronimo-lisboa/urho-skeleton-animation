using System;
using System.Diagnostics;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace SkeletalAnimation
{
    /// <summary>
    /// Um teste de como se usa animação esqueletal.
    /// A primeira etapa é fazer o original funcionar.
    /// A segunda etapa é eu mesmo criar uma animação e ela ser usada.
    ///
    /// //TODO: Criar minha propria animação e usá-la
    /// </summary>
    public class MyGame : Application
    {
        //modelMoveSpeed, modelRotateSpeed, bounds
        public float ModelMoveSpeed { get; set; }
        public float ModelRotationSpeed { get; set; }
        public BoundingBox Bounds { get; private set; }
        private Node CameraNode;
        private Camera camera;
        private Node Dude;

        [Preserve]
        public MyGame(ApplicationOptions options) : base(options) { }

        private Scene _scene;

        static MyGame()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                e.Handled = true;
            };
        }

        protected override async void Start()
        {
            base.Start();
            CreateWorld();
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            CameraNode.LookAt(Dude.Position, new Vector3(0, 1, 0));
        }

        private void CreateWorld()
        {
            Bounds = new BoundingBox(new Vector3(-47.0f, 0.0f, -47.0f), new Vector3(47.0f, 0.0f, 47.0f));
            ModelMoveSpeed = 2.0f;
            ModelRotationSpeed = 100.0f;

            _scene = CreateScene();            
            CreatePlane(_scene);
            CreateZone(_scene);
            CreateDirectionalLight(_scene);
            Dude = CreateDude(_scene);
            CreateCamera(_scene);

            this.Renderer.SetViewport(0, new Viewport(Context, _scene, camera, null));
        }
        /// <summary>
        /// Onde eu crio a câmera
        /// </summary>
        /// <param name="scene"></param>
        private void CreateCamera(Scene scene)
        {
            CameraNode = scene.CreateChild("Camera");
            camera = CameraNode.CreateComponent<Camera>();
            camera.FarClip = 300;
            // Set an initial position for the camera scene node above the plane
            CameraNode.Position = new Vector3(0.0f, 5.0f, 0.0f);
        }

        
        /// <summary>
        /// Onde eu crio o maluco que fica andando pelo mapa, animado por uma aniamação esqueletal
        /// </summary>
        /// <param name="scene"></param>
        private Node CreateDude(Scene scene)
        {
            var cache = this.ResourceCache;
            var modelNode = scene.CreateChild("Jack");
            modelNode.Position = new Vector3(0,0,0);
            modelNode.Rotation = new Quaternion(0, 0, 0);
            //var modelObject = modelNode.CreateComponent<AnimatedModel>();
            var modelObject = new AnimatedModel();
            modelNode.AddComponent(modelObject);
            modelObject.Model = cache.GetModel("Models/Jack.mdl");
            //modelObject.Material = cache.GetMaterial("Materials/Jack.xml");
            modelObject.CastShadows = true;

            // Create an AnimationState for a walk animation. Its time position will need to be manually updated to advance the
            // animation, The alternative would be to use an AnimationController component which updates the animation automatically,
            // but we need to update the model's position manually in any case
            var walkAnimation = cache.GetAnimation("Models/Jack_Walk.ani");
            var state = modelObject.AddAnimationState(walkAnimation);
            // The state would fail to create (return null) if the animation was not found
            if (state != null)
            {
                // Enable full blending weight and looping
                state.Weight = 1;
                state.Looped = true;
            }

            // Create our custom Mover component that will move & animate the model during each frame's update
            var mover = new Mover(ModelMoveSpeed, ModelRotationSpeed, Bounds);
            modelNode.AddComponent(mover);
            return modelNode;
        }

 

        /// <summary>
        /// Onde eu crio a luz direcional, com sombras ativas
        /// </summary>
        /// <param name="scene"></param>
        private void CreateDirectionalLight(Scene scene)
        {
            var lightNode = scene.CreateChild("DirectionalLight");
            lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f));
            var light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Directional;
            light.CastShadows = true;
            light.ShadowBias = new BiasParameters(0.00025f, 0.5f);
            light.ShadowCascade = new CascadeParameters(10.0f, 50.0f, 200.0f, 0.0f, 0.8f);
        }



        /// <summary>
        /// Cria uma Zona. Uma zona é uma região com luz ambiente e névoa própria
        /// </summary>
        /// <param name="scene"></param>
        private void CreateZone(Scene scene)
        {
            var zoneNode = scene.CreateChild("Zone");
            var zone = zoneNode.CreateComponent<Zone>();
            zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));//O tamanho default da octree é 1000x1000
            zone.AmbientColor = new Color(0.15f, 0.15f, 0.15f);
            zone.FogColor = new Color(0.5f, 0.5f, 0.7f);
            zone.FogStart = 100;
            zone.FogEnd = 300;
        }

        /// <summary>
        /// É aqui que eu crio o plano onde os bonecos vão andar.
        /// </summary>
        /// <param name="scene"></param>
        void CreatePlane(Scene scene)
        {
            var cache = this.ResourceCache;
            var planeNode = scene.CreateChild("Plane");
            planeNode.Scale = new Vector3(100, 1, 100);
            var planeObject = planeNode.CreateComponent<StaticModel>();
            planeObject.Model = cache.GetModel("Models/Plane.mdl");
            planeObject.SetMaterial(cache.GetMaterial("Materials/Grass.xml"));
        }

        /// <summary>
        /// Cria o objeto de cena.
        /// </summary>
        /// <returns></returns>
        private Scene CreateScene()
        {
            var s = new Scene();
            s.CreateComponent<Octree>();
            s.CreateComponent<DebugRenderer>();
            return s;
        }

    }
}
