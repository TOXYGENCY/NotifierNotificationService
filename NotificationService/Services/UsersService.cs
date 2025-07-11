using Microsoft.AspNetCore.Identity;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository usersRepository;
        private readonly PasswordHasher<object> hasher; // object, а не User, потому что в этой реализации .HashPassword аргумент newUserDto не используется

        public UsersService(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
            hasher = new PasswordHasher<object>();
        }

        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            var result = hasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                return true;
            else
                return false;
        }

        // хеширование пароля
        public string HashPassword(string password)
        {
            return hasher.HashPassword(null, password); // null, а не User, потому что в этой реализации .HashPassword аргумент newUserDto не используется
        }

        public async Task AddUserAsync(UserDto newUserDto, string password)
        {
            if (newUserDto is null) throw new ArgumentNullException(nameof(newUserDto));
            var existingUser = await usersRepository.GetByLoginAsync(newUserDto.Login);
            if (existingUser != null) throw new ArgumentException(newUserDto.Login);

            var newUser = FromDto(newUserDto);
            newUser.PasswordHash = HashPassword(password);

            await usersRepository.AddAsync(newUser);
        }

        public async Task UpdateUserAsync(Guid userId, UserDto updatedUserDto, string? newPassword = null)
        {
            if (updatedUserDto is null || userId == Guid.Empty)
                throw new ArgumentNullException(nameof(updatedUserDto));

            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Пользователь с Id = {userId} не найден.");

            if (!string.IsNullOrEmpty(updatedUserDto.Login))
                user.Login = updatedUserDto.Login;

            if (!string.IsNullOrEmpty(newPassword))
                user.PasswordHash = HashPassword(newPassword);

            await usersRepository.UpdateAsync(user);
        }

        public async Task<User?> FromDtoToEntityAsync(Guid userId, UserDto? userDto)
        {
            // Сразу ищем в контексте.
            var user = await usersRepository.GetByIdAsync(userId);

            // Если dto не передан, то с ним работать не можем - возвращаем что есть
            if (userDto is null) return user;

            if (!String.IsNullOrEmpty(userDto.Login))
                user ??= await usersRepository.GetByLoginAsync(userDto.Login);

            // Если dto есть и нет объекта в БД - конвертируем
            user ??= FromDto(userDto, user);
            return user;
        }

        public User? FromDto(UserDto? userDto, User? baseForDto = null)
        {
            if (userDto is null)
                return null;

            // Создаём нового пользователя
            var user = new User
            {
                Login = userDto.Login ?? "",
                // Пароля нет в DTO
                NotificationRecipientUsers = new List<Notification>(),
                NotificationSenderUsers = new List<Notification>()
            };

            if (baseForDto != null)
            {
                user.Login = baseForDto.Login;
                user.PasswordHash = baseForDto.PasswordHash;
                // Коллекции не трогаем - они остаются как есть
            }

            return user;
        }

        public UserDto? ToDto(User? user)
        {
            return JsonSerializationConvert<User, UserDto>(user);
        }

        public IEnumerable<UserDto>? ToDtos(IEnumerable<User>? users)
        {
            if (users is null) return null;
            var userDtos = new List<UserDto>();

            foreach (var user in users)
                userDtos.Add(ToDto(user));

            return userDtos;
        }

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект (встроенный авто-маппинг).
        /// </summary>
        /// <typeparam name="SRC"></typeparam>
        /// <typeparam name="DEST"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        private DEST? JsonSerializationConvert<SRC, DEST>(SRC? src)
        {
            if (src == null) return default(DEST);
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await usersRepository.GetByIdAsync(userId);
            return ToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await usersRepository.GetAllAsync();
            return ToDtos(users);
        }
    }
}