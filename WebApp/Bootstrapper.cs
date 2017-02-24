using System;

using Nancy;
using Nancy.Conventions;

namespace TrulyQuantumChess.WebApp {
    public class Bootstrapper : DefaultNancyBootstrapper {
        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines) {
            base.ApplicationStartup(container, pipelines);

            Conventions.ViewLocationConventions.Clear();
            Conventions.ViewLocationConventions.Add((viewName, model, context) => {
                return String.Concat("Templates/", viewName);
            });
            Conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory(WebAppConfig.Instance.Prefix + "/content", "/Content")
            );
        }
    }
}