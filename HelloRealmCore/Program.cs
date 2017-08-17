using System;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Realms;
using Realms.Sync;

namespace HelloRealmCore
{
    class Program
    {
		private const string Username = "a@a";
		private const string Password = "a";
		private const string ServerUrl = "localhost:9080";

		private static Realm _realm;
        private static IDisposable _notificationToken;

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Realm...");

            var filename = args.FirstOrDefault() ?? "numberRealm";
            InitializeRealm(filename);
        }

        static void InitializeRealm(string filename)
        {
            AsyncContext.Run(async () =>
            {
                try
                {
					var credentials = Credentials.UsernamePassword(Username, Password, createUser: false);
					var user = await User.LoginAsync(credentials, new Uri($"http://{ServerUrl}"));
					var config = new SyncConfiguration(user, new Uri($"realm://{ServerUrl}/~/numberRealm"), filename);
					_realm = await Realm.GetInstanceAsync(config);
					_notificationToken = _realm.All<Number>().SubscribeForNotifications((sender, changes, error) =>
					{
                        if (sender != null && sender.Any())
                        {
							Console.WriteLine("Collection updated...");
							Console.WriteLine(string.Join(", ", sender.Select(s => s.Value)));
						}
      				});

                    Console.Write("Realm initialized. Input a number or 'exit' to quit: ");

					while (true)
					{
                        // Need to get input async otherwise the thread blocks and
                        // notifications are not delivered automatically.
                        var input = await Task.Run(() =>
                        {
                            return Console.ReadLine();
                        });

						if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
						{
							break;
						}

						if (!int.TryParse(input, out var number))
						{
							Console.WriteLine($"{input} is not a number!");
						}
                        else
                        {
							_realm.Write(() =>
							{
								_realm.Add(new Number { Value = number });
							});
						}
					}
				}
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }
    }

    class Number : RealmObject
    {
        public int Value { get; set; }
    }
}
