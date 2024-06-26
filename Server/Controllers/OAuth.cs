﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Server.Controllers
{
    public class OAuth : Controller
    {
        [HttpGet]
        public IActionResult Authorize(
            string response_type, //authorization flow type
            string client_id,
            string redirect_uri,  
            string scope, // what info i want like email, grandma, tel
            string state) // random string generated to confirm we are going to back to the same client
        {
            // ?a=foo&b=bar
            var query = new QueryBuilder();
            query.Add("redirectUri", redirect_uri);
            query.Add("state", state);

            return View(model: query.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(
            string username, 
            string redirectUri, 
            string state)
        {
            const string code = "LKJBASDGFLKJB";

            var query = new QueryBuilder();
            query.Add("code", code);
            query.Add("state", state);

            return Redirect($"{redirectUri}{query.ToString()}");
        }

        public async Task<IActionResult> Token(
            string grant_type, //flow of access_token request
            string code, //confirmation of the authentication process
            string redirect_uri,
            string client_id,
            string refresh_token)
        {
            //some mechanism for validating the code

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
                new Claim("granny", "cookie")
            };

            var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(Constants.Issuer,
                Constants.Audience,
                claims,
                notBefore: DateTime.Now,
                expires: grant_type == "refresh_token" ? DateTime.Now.AddMinutes(5) : DateTime.Now.AddMilliseconds(1),
                signingCredentials);

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token,
                token_type = "Bearer",
                raw_claim = "oauthTutorial",
                refresh_token = "RefreshTokenSampleValueSomething77"
            };

            var responseJson = JsonConvert.SerializeObject(responseObject);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);

            await Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);

            return Redirect(redirect_uri);
        }

        [Authorize]
        public IActionResult Validate()
        {
            if (HttpContext.Request.Query.TryGetValue("access_token", out var accessToken))
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
