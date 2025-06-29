using Microsoft.AspNetCore.Identity;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
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
            // Проверка данных и выдача at + rt + данных пользователя, если успех
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

        public async Task UpdateUserAsync(Guid userId, UserDto updatedUser, string? newPassword = null)
        {
            if (updatedUser == null || userId == null) throw new ArgumentNullException("Не все аргументы переданы.");
            if (updatedUser.Id != userId) throw new ArgumentException("ID не совпадают.");

            // Получаем текущего пользователя из репозитория
            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null) throw new InvalidOperationException($"Пользователь с Id = {userId} не найден.");

            // Обновляем поля, которые должны измениться
            user.Login = updatedUser.Login;

            // Обновляем пароль, если он был передан
            if (!string.IsNullOrWhiteSpace(newPassword))
                // Хешируем пароль перед сохранением
                user.PasswordHash = HashPassword(newPassword);

            await usersRepository.UpdateAsync(user);
        }

        public async Task AddUserAsync(UserDto newUserDto, string password)
        {
            if (newUserDto is null) throw new ArgumentNullException(nameof(newUserDto));
            var existingUser = await usersRepository.GetByLoginAsync(newUserDto.Login);
            if (existingUser != null) throw new ArgumentException(newUserDto.Login);

            var newUser = await FromDtoAsync(newUserDto);
            newUser.PasswordHash = HashPassword(password);

            await usersRepository.AddAsync(newUser);
        }

        public async Task<User> FromDtoAsync(UserDto userDto)
        {
            if (userDto is null) throw new ArgumentNullException(nameof(userDto));

            var user = await usersRepository.GetByIdAsync(userDto.Id);

            user ??= JsonSerializationConvert<UserDto, User>(userDto);
            return user;
        }

        public UserDto ToDto(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return JsonSerializationConvert<User, UserDto>(user);
        }

        public IEnumerable<UserDto> ToDtos(IEnumerable<User> users)
        {
            if (users is null) throw new ArgumentNullException(nameof(users));

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
        private DEST JsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
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