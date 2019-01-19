using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Graph;

namespace BotEngine.Services
{
  public class GraphClient
  {
    private readonly string _token;

    public GraphClient(string token)
    {
      _token = token;
    }

    public async Task<User> GetMe()
    {
      var graphClient = GetAuthenticatedClient();
      var me = await graphClient.Me.Request().GetAsync();
      return me;
    }

    public async Task<IList<Person>> GetPeople(string name)
    {
      try
      {
        var options = new[] {
                               // new QueryOption("$filter",$"startswith(givenName,'{name}')")
                                new QueryOption("$filter",CreateNameQuery(name))
                                };

        //startswith(displayName,'mary') or startswith(givenName,'mary') 
        //or startswith(surname,'mary') or startswith(mail,'mary') or startswith(userPrincipalName,'mary')

        var graphClient = GetAuthenticatedClient();
        var people = await graphClient.Me.People.Request(options).GetAsync();

        //var people = graphClient.Users.Request().Top(5).GetAsync().GetAwaiter().GetResult();

        return people.CurrentPage;
      }
      catch (Exception ex)
      {

        throw ex;
      }

    }

    public string CreateNameQuery(string personName)
    {
      var fname = "";
      var lname = "";

      var splitNameResult = personName.Split(' ');

      if (splitNameResult.Length > 1)
      {
        fname = splitNameResult[0];
        lname = splitNameResult[1];
      }
      else
      {
        fname = lname = personName;
      }

      string nameQuery = $"startswith(givenName,'{fname}')";

      nameQuery += $" or startswith(displayName,'{fname}')";
      nameQuery += $" or startswith(surname,'{fname}')";

      return nameQuery;
    }

    private GraphServiceClient GetAuthenticatedClient()
    {
      GraphServiceClient graphClient = new GraphServiceClient(
          new DelegateAuthenticationProvider(
              async (requestMessage) =>
              {
                string accessToken = _token;

                      // Append the access token to the request.
                      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                      // Get event times in the current time zone.
                      requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");
              }));
      return graphClient;
    }
  }
}