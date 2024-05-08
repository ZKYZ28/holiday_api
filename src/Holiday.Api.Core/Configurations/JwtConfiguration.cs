﻿namespace Holiday.Api.Core.Configurations;

public sealed class JwtConfiguration
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string SecretKey { get; set; } = "";
}