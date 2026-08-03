using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; // 为控制器类提供基础功能
using Microsoft.EntityFrameworkCore; // 支持与数据库交互
using Microsoft.IdentityModel.Tokens; // 提供 JWT 相关功能
using System.IdentityModel.Tokens.Jwt; // 提供生成和解析 JWT 的功能
using System.Security.Claims; // 提供声明（Claims）的功能
using System.Security.Cryptography; // 提供密码学支持
using System.Text; // 支持字符串操作
using System.Threading.Tasks; // 支持异步操作
using web_api2022.Controllers;
using web_api2022.Data;
using web_api2022.Models; // 引用项目中的数据上下文

[Route("api/[controller]")] // 定义路由前缀为 api/Auth
[ApiController] // 标记此类为 Web API 控制器
public class AuthController : ControllerBase
{
    private readonly DataContext _context; // 用于数据库操作
    private readonly IConfiguration _configuration; // 用于加载应用配置

    // 构造函数，通过依赖注入获取 DataContext 和 IConfiguration
    public AuthController(DataContext context, IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); // 确保 context 不为空
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)); // 确保 configuration 不为空
    }

    /// <summary>
    /// 用户登录并生成 Token
    /// </summary>
    [HttpPost("Api_Login")] // 定义路由为 api/Auth/Api_Login
    public async Task<IActionResult> Login_api([FromBody] LoginRequest loginRequest)
    {
        // 检查请求是否有效
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest(new { State = 1, Message = "Invalid request payload." }); // 如果无效，返回 400 错误
        }

        // 在数据库中查询用户
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

        // 验证用户是否存在以及密码是否正确
        if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            return Unauthorized(new { State = 1, Message = "Invalid username or password" }); // 如果失败，返回 401 错误
        }

        // 如果验证通过，生成 Token
        var token = GenerateJwtToken(user.Username);

        // 当前时间戳
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 计算过期时间,config中设定为3小时,3*3600=3h
        int expirationSeconds = Convert.ToInt32(_configuration["Jwt:ExpirationHours"]) * 3600;
        //return Ok(new { success = true });

        return Ok(new { State = 0, expires_In = Convert.ToInt32( _configuration["Jwt:ExpirationHours"])*3600, expiresIn = DateTime.UtcNow.AddSeconds(expirationSeconds), Token = token }); // 返回 Token
    }


    /// <summary>
    /// 前端网页登录
    /// </summary>
    /// <param name="loginRequest"></param>
    /// <returns></returns>
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            // 验证用户是否存在
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

            if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized(new { success = false, message = "用户名或密码错误" });
            }

            // 生成 JWT Token
            var token = GenerateJwtToken(user.Username);

            return Ok(new
            {
                success = true,
                message = "登录成功",
                token = token
            });
        }
        catch (Exception ex)
        {
            // 记录日志
            Console.WriteLine($"Login error: {ex.Message}");
            return StatusCode(500, new { success = false, message = "服务器内部错误" });
        }
    }
    /// <summary>
    /// 获取受保护资源（示例）
    /// </summary>
    [Authorize] // 表示此方法需要有效的 Token 才能访问
    [HttpGet("Protected")] // 定义路由为 api/Auth/Protected
    public IActionResult GetProtectedData()
    {
        var username = User.Identity?.Name; // 从 Token 的声明中提取用户名
        return Ok(new { Message = $"Welcome {username}, this is protected data!" }); // 返回欢迎消息
    }

    /// <summary>
    /// 生成 JWT Token
    /// </summary>
    private string GenerateJwtToken(string username)
    {
        // 从配置中读取 JWT 设置
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expirationHours = int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 3;

        // 检查配置是否完整
        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            throw new InvalidOperationException("JWT configuration is missing or invalid."); // 如果不完整，抛出异常
        }

        // 创建声明，用于在 Token 中存储用户信息
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username), // 用户名作为主题
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // 唯一的 Token ID
            new Claim(ClaimTypes.Name, username) // 用户名声明
        };

        // 创建对称密钥
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // 使用 HMAC-SHA256 签名算法

        // 创建 Token
        var token = new JwtSecurityToken(
            issuer: issuer, // 设置发行者
            audience: audience, // 设置受众
            claims: claims, // 设置声明
            expires: DateTime.UtcNow.AddHours(expirationHours), // 设置过期时间
            signingCredentials: creds // 设置签名凭据
        );

        return new JwtSecurityTokenHandler().WriteToken(token); // 返回生成的 Token 字符串
    }

    /// <summary>
    /// 验证密码是否正确
    /// </summary>
    private bool VerifyPassword(string password, string storedHash)
    {
        using (var sha256 = SHA256.Create()) // 使用 SHA256 哈希算法
        {
            var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))); // 计算哈希
            return computedHash == storedHash; // 将计算出的哈希与存储的哈希进行比较
        }
    }

    /// <summary>
    /// 注册用户（示例）添加新用户 
    /// </summary>
    [HttpPost("Register")] // 定义路由为 api/Auth/Register
public async Task<IActionResult> Register([FromBody] LoginRequest registerRequest)
{
    try
    {
        if (registerRequest == null || string.IsNullOrWhiteSpace(registerRequest.Username) || string.IsNullOrWhiteSpace(registerRequest.Password))
        {
            return BadRequest(new { Message = "Invalid registration request. Username and password are required." });
        }

        // 检查用户名是否已存在
        if (await _context.Users.AnyAsync(u => u.Username == registerRequest.Username))
        {
            return Conflict(new { Message = "Username already exists." });
        }

        // 创建新用户
        var newUser = new UserModel
        {
            Username = registerRequest.Username,
            PasswordHash = HashPassword(registerRequest.Password)
        };

        // 将新用户添加到数据库
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // 返回用户数据
        return Ok(new { Message = "User registered successfully.", User = newUser });
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"Database update error: {ex.Message}");
        return StatusCode(500, new { Message = "An error occurred while saving the user to the database." });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return StatusCode(500, new { Message = "An unexpected error occurred during registration." });
    }
}


    /// <summary>
    /// 哈希密码  将输入的明文密码转换成哈希密码
    /// </summary>
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create()) // 使用 SHA256 哈希算法
        {
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))); // 返回加密后的密码
        }
    }
}

/// <summary>
/// 登录请求模型
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } // 用户名
    public string Password { get; set; } // 密码
}
