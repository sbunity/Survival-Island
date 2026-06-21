using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Save Controller", core: true, order: 900)]
    public class SaveInitModule : InitModule
    {
        public override string ModuleName => "Save Controller";

        [SerializeField] float autoSaveDelay = 0;

        [Space]
        [SerializeField] string webGLPrefix = "gameName";

        public override void CreateComponent()
        {
            SaveController saveController = Initializer.GameObject.AddComponent<SaveController>();

            saveController.Configure(new SaveWrapperConfig
            {
                WebGLPrefix = webGLPrefix
            });

            saveController.StartCoroutine(saveController.InitAsync(autoSaveDelay));
        }
    }
}
