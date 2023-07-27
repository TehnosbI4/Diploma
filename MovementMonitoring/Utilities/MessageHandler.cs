using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MovementMonitoring.Data;
using MovementMonitoring.Hubs;
using MovementMonitoring.Models;
using System.Text.Json;

namespace MovementMonitoring.Utilities
{
    public class MessageHandler
    {
        private record DetectedPerson(string Guid, string LastPhotoPath, bool Validated, string MostSimilarGuid, string MostSimilarPhotoPath, float Similarity);
        private record Message(string SourceId, string Time, float ValidationThreshold, List<DetectedPerson> DetectedPersons);
        private readonly EntityDBContext _context;
        private readonly IHubContext<ViolationNotificationHub, IViolationNotification> _notContext;

        public MessageHandler(string stringMessage,
                        EntityDBContext context,
                        IHubContext<ViolationNotificationHub, IViolationNotification> notContext)
        {
            _context = context;
            _notContext = notContext;
            _ = DecodeMessage(stringMessage);
        }

        public async Task DecodeMessage(string stringMessage)
        {
            Console.WriteLine($" [x] Received {stringMessage}");
            Message? message = JsonSerializer.Deserialize<Message>(stringMessage);

            if (message != null)
            {
                Camera? camera = _context.Cameras.FirstOrDefault(x => x.Id.ToString() == message.SourceId);
                if (camera != null)
                {
                    string[] date = message.Time.Split("-");
                    int[] time = date[3].Split(".").Select(x => int.Parse(x)).ToArray();
                    DateTime dateTime = new(year: int.Parse(date[0]),
                                            month: int.Parse(date[1]),
                                            day: int.Parse(date[2]),
                                            hour: time[0],
                                            minute: time[1],
                                            second: time[2],
                                            millisecond: time[3] / 1000,
                                            microsecond: time[3] % 1000);
                    string description = $"Обнаружен в '{message.Time}' в помещении '{camera.Room!.Name}' видеокамерой '{camera.Id}'.";
                    foreach (DetectedPerson detectedPerson in message.DetectedPersons)
                    {
                        await RegisterDetectedPersonAsync(camera, dateTime, detectedPerson, description);
                    }
                }
                else
                {
                    Console.WriteLine($"Camera {message.SourceId} does not exist!");
                }
            }
        }

        private async Task RegisterDetectedPersonAsync(Camera camera, DateTime dateTime, DetectedPerson detectedPerson, string defaultescription)
        {
            Person currentPerson = await GetPersonAsync(detectedPerson.Guid, defaultescription);
            Person mostSimilarPerson;
            if (detectedPerson.Guid == detectedPerson.MostSimilarGuid)
            {
                mostSimilarPerson = currentPerson;
            }
            else
            {
                mostSimilarPerson = await GetPersonAsync(detectedPerson.MostSimilarGuid, defaultescription);
            }

            Movement? lastMovement = _context.Movements.OrderBy(x => x.Id).LastOrDefault(x => x.CurrentPerson!.Guid == currentPerson.Guid);

            if (lastMovement != null)
            {
                if (lastMovement.Room!.Id != camera.Room!.Id)
                {
                    lastMovement.LeavingTime = dateTime;
                    Movement movement = new()
                    {
                        Camera = camera,
                        Room = camera.Room,
                        EnteringTime = dateTime,
                        LastDetectionTime = dateTime,
                        CurrentPerson = currentPerson,
                        MostSimilarPerson = mostSimilarPerson,
                        MostSimilarPhotoPath = detectedPerson.MostSimilarPhotoPath,
                        FirstDetectionSimilarity = detectedPerson.Similarity,
                        LastDetectionSimilarity = detectedPerson.Similarity,
                        LastPhotoPath = detectedPerson.LastPhotoPath,
                    };
                    await SetMovementAsync(movement, dateTime);
                }
                else
                {
                    lastMovement.LastDetectionTime = dateTime;
                    lastMovement.LastDetectionSimilarity = detectedPerson.Similarity;
                }
                await SetMovementAsync(lastMovement, dateTime);
            }
            else
            {
                Movement movement = new()
                {
                    Camera = camera,
                    Room = camera.Room,
                    EnteringTime = dateTime,
                    LastDetectionTime = dateTime,
                    CurrentPerson = currentPerson,
                    MostSimilarPerson = mostSimilarPerson,
                    MostSimilarPhotoPath = detectedPerson.MostSimilarPhotoPath,
                    FirstDetectionSimilarity = detectedPerson.Similarity,
                    LastDetectionSimilarity = detectedPerson.Similarity,
                    LastPhotoPath = detectedPerson.LastPhotoPath,
                };
                await SetMovementAsync(movement, dateTime);
            }
            await _context.SaveChangesAsync();
            TablesUpdateList.SetTableUpdateRequest("Movement");
        }

        private async Task<Person> GetPersonAsync(string guid, string description)
        {
            Person? person = _context.Persons.FirstOrDefault(x => x.Guid == guid);
            if (person == null)
            {
                person = new()
                {
                    Guid = guid,
                    Name = "Anonymous " + guid[..8],
                    Description = description,
                    AccessLevel = _context.AccessLevels.OrderBy(x => x.Id).FirstOrDefault()
                };
                _context.Persons.Add(person);
                await _context.SaveChangesAsync();
                TablesUpdateList.SetTableUpdateRequest("Person");
            }
            return person;
        }

        //private static bool IsViolation(Person person, Room room, DateTime dateTime)
        //{
        //    if (person.AccessLevel!.Id >= room.AccessLevel!.Id)
        //    {
        //        TimeSpan startTime = person.AccessLevel!.StartTime.TimeOfDay;
        //        TimeSpan endTime = person.AccessLevel!.EndTime.TimeOfDay;
        //        bool isViolation = startTime != endTime && (dateTime.TimeOfDay < startTime || dateTime.TimeOfDay > endTime);
        //        return isViolation;
        //    }
        //    return true;
        //}

        private async Task SetMovementAsync(Movement movement, DateTime dateTime)
        {
            if (!movement.IsViolation)
            {
                string type;
                bool isViolation;
                if (movement.CurrentPerson!.AccessLevel!.Id >= movement.Room!.AccessLevel!.Id)
                {
                    TimeSpan startTime = movement.CurrentPerson!.AccessLevel!.StartTime.TimeOfDay;
                    TimeSpan endTime = movement.CurrentPerson!.AccessLevel!.EndTime.TimeOfDay;
                    isViolation = startTime != endTime && (dateTime.TimeOfDay < startTime || dateTime.TimeOfDay > endTime);
                    type = "Нарушение расписания";
                }
                else
                {
                    isViolation = true;
                    type = "Нарушение уровня доступа";
                }
                movement.IsViolation = isViolation;
                if (_context.Movements.Contains(movement))
                {
                    if (movement.IsViolation != isViolation)
                    {
                        _context.Attach(movement).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
                else
                {
                    _context.Add(movement);
                    _context.SaveChanges();
                }
                if (isViolation)
                {
                    Violation violation = new() { Movement = movement, DateTime = dateTime, Type = type };
                    _context.Add(violation);
                    _context.SaveChanges();
                    Violation lastViolation = _context.Violations.Where(x => x.Movement!.Id == movement.Id).OrderByDescending(x => x.Id).First();
                    await _notContext.Clients.All.ViolationNotify(lastViolation.Id.ToString(), dateTime.ToString(), movement.Room!.Name!);
                    TablesUpdateList.SetTableUpdateRequest("Violation");
                }
            }
        }
    }
}
