using Microsoft.AspNetCore.Mvc;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces;

namespace NotifierNotificationService.NotificationService.Controllers

{
    [Route("api/v1/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository usersRepository;
        private readonly IUsersService usersService;
        private readonly ILogger<UsersController> logger;

        public UsersController(IUsersRepository usersRepository,
            ILogger<UsersController> logger,
            IUsersService usersService)
        {
            this.usersRepository = usersRepository;
            this.logger = logger;
            this.usersService = usersService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                var users = await usersRepository.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while retrieving all users.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при получении всех пользователей. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> GetUserById(Guid userId)
        {
            try
            {
                var user = await usersRepository.GetByIdAsync(userId);
                if (user == null) return StatusCode(StatusCodes.Status404NotFound);

                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while searching for the user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при поиске пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserAsync(Guid userId, User updatedUser,
                                                            string? newPassword = null)
        {
            try
            {
                await usersService.UpdateUserAsync(userId, updatedUser, newPassword);
                logger.LogInformation($"User {updatedUser.Login} updated");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, "User not found");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Пользователь не найден.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while updating the user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при обновлении пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }


        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            {
                await usersRepository.DeleteAsync(userId);
                logger.LogInformation($"User with id = {userId} has been deleted");

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while deleting the user.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при удалении пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddUserAsync(User User, string password)
        {
            try
            {
                if (User is null)
                {
                    throw new ArgumentNullException(nameof(User));
                }

                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException($"'{nameof(password)}' cannot be null or empty.", nameof(password));
                }

                await usersService.AddUserAsync(User, password);
                logger.LogInformation($"User {User.Login} created");

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "Required data to add a user is not received.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Необходимые данные для добавления пользователя не получены.Обратитесь к администратору или попробуйте позже.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding the user.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при добавлении пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }
    }
}
