﻿using System.ComponentModel.DataAnnotations;

namespace JWT.RequestModels;

public class RegisterRequestModel
{
    [Required] [EmailAddress] public string UserName { get; set; }

    [Required] public string Password { get; set; }
}