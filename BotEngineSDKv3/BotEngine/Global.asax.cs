using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System.Configuration;
using System.Reflection;
using System.Web.Http;

namespace BotEngine
{
  public class WebApiApplication : System.Web.HttpApplication
  {
    protected void Application_Start()
    {
      var store = new InMemoryDataStore();

      Conversation.UpdateContainer(
                 builder =>
                 {
                   builder.Register(c => store)
                             .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                             .AsSelf()
                             .SingleInstance();

                   builder.Register(c => new CachingBotDataStore(store,
                              CachingBotDataStoreConsistencyPolicy
                              .ETagBasedConsistency))
                              .As<IBotDataStore<BotData>>()
                              .AsSelf()
                              .InstancePerLifetimeScope();
                 });

      ///*Azure storage*/
      //Conversation.UpdateContainer(
      //  builder =>
      //  {
      //    builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));
      //    var store = new TableBotDataStore(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

      //    builder.Register(c => store)
      //    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
      //    .AsSelf()
      //    .SingleInstance();
      //  });

      GlobalConfiguration.Configure(WebApiConfig.Register);
    }

    private void RegisterBotModules()
    {
      Conversation.UpdateContainer(builder =>
      {
        builder.RegisterModule(new ReflectionSurrogateModule());
        builder.RegisterModule<GlobalMessageHandlersBotModule>();
      });
    }
  }
}
