using MovementMonitoring.Models;
using MovementMonitoring.Utilities;
using System.Dynamic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MovementMonitoring.Data
{
    public static class DbInitializer
    {
        public static void Initialize(EntityDBContext context)
        {
            if (context.AccessLevels.Any())
            {
                return;
            }
            DateTime StartTime = new(1, 1, 1, 10, 0, 0);
            DateTime endTime = new(1, 1, 1, 18, 0, 0);
            AccessLevel accessLevel = new() { Name = "Нулевой уровень доступа", Description = "Выдается абсолютно всем людям, попавшим в зону функционирования системы", StartTime = StartTime, EndTime = endTime };
            AccessLevel accessLevel1 = new() { Name = "Первый уровень доступа", Description = "Выдается сотрудникам младшего звена", StartTime = StartTime, EndTime = endTime };
            AccessLevel[] accessLevels = new[]
            {
                accessLevel,
                accessLevel1,
                new() { Name = "Второй уровень доступа", Description = "Выдается сотрудникам среднего звена", StartTime = StartTime, EndTime = endTime },
                new() {Name = "Третий уровень доступа", Description = "Выдается сотрудникам высшего звена", StartTime = StartTime, EndTime = endTime}
            };

            Room room = new() { Name = "Первая комната", Description = "Комната...", AccessLevel = accessLevel };

            dynamic myConfig = Configurator.YamlConfig;

			List<Camera> cameras = new List<Camera>();
            for (int i = 1; i <= myConfig.sources.Count; i++)
            {
                cameras.Add(new() { Name = $"Камера комнаты {i}", Description = "камера...", Room = room });
            }

            context.AccessLevels.AddRange(accessLevels);
            context.Rooms.AddRange(room);
            context.Cameras.AddRange(cameras);

            //List<AccessLevel> accessLevelsList = new List<AccessLevel>();
            //for (int i = 0; i < 5000; i++)
            //{
            //    accessLevelsList.Add(new() { Name = i.ToString(), Description = i.ToString(), StartTime = StartTime, EndTime = endTime });
            //}
            //context.AccessLevels.AddRange(accessLevelsList);


            context.SaveChanges();
        }
    }
}
