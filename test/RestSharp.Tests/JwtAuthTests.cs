﻿using System.Globalization;
using RestSharp.Authenticators;

namespace RestSharp.Tests; 

public class JwtAuthTests {
    readonly string _testJwt;
    readonly string _expectedAuthHeaderContent;

    public JwtAuthTests() {
        Thread.CurrentThread.CurrentCulture   = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;

        _testJwt = "eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9" +
            "." +
            "eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQo" +
            "gImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ" +
            "." +
            "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

        _expectedAuthHeaderContent = $"Bearer {_testJwt}";
    }

    [Fact]
    public void Can_Set_ValidFormat_Auth_Header() {
        var client  = new RestClient { Authenticator = new JwtAuthenticator(_testJwt) };
        var request = new RestRequest();

        //In real case client.Execute(request) will invoke Authenticate method
        client.Authenticator.Authenticate(client, request);

        var authParam = request.Parameters.Single(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

        Assert.True(authParam.Type == ParameterType.HttpHeader);
        Assert.Equal(_expectedAuthHeaderContent, authParam.Value);
    }

    [Fact]
    public void Check_Only_Header_Authorization() {
        var client  = new RestClient { Authenticator = new JwtAuthenticator(_testJwt) };
        var request = new RestRequest();

        // Paranoid server needs "two-factor authentication": jwt header and query param key for example
        request.AddParameter("Authorization", "manualAuth", ParameterType.QueryString);

        // In real case client.Execute(request) will invoke Authenticate method
        client.Authenticator.Authenticate(client, request);

        var paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

        Assert.Equal(2, paramList.Count);

        var queryAuthParam  = paramList.Single(p => p.Type.Equals(ParameterType.QueryString));
        var headerAuthParam = paramList.Single(p => p.Type.Equals(ParameterType.HttpHeader));

        Assert.Equal("manualAuth", queryAuthParam.Value);
        Assert.Equal(_expectedAuthHeaderContent, headerAuthParam.Value);
    }

    [Fact]
    public void Set_Auth_Header_Only_Once() {
        var client  = new RestClient();
        var request = new RestRequest();

        request.AddHeader("Authorization", "second_header_auth_token");

        client.Authenticator = new JwtAuthenticator(_testJwt);

        //In real case client.Execute(...) will invoke Authenticate method
        client.Authenticator.Authenticate(client, request);

        var paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

        Assert.Equal(1, paramList.Count);

        var authParam = paramList[0];

        Assert.True(authParam.Type == ParameterType.HttpHeader);
        Assert.Equal(_expectedAuthHeaderContent, authParam.Value);
        Assert.NotEqual("Bearer second_header_auth_token", authParam.Value);
    }

    [Fact]
    public void Updates_Auth_Header() {
        var client  = new RestClient();
        var request = new RestRequest();

        var authenticator = new JwtAuthenticator(_expectedAuthHeaderContent);

        client.Authenticator = authenticator;
        client.Authenticator.Authenticate(client, request);

        authenticator.SetBearerToken("second_header_auth_token");
        client.Authenticator.Authenticate(client, request);

        var paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

        Assert.Single(paramList);

        var authParam = paramList[0];

        Assert.True(authParam.Type == ParameterType.HttpHeader);
        Assert.NotEqual(_expectedAuthHeaderContent, authParam.Value);
        Assert.Equal("Bearer second_header_auth_token", authParam.Value);
    }

    [Fact]
    public void Throw_Argument_Null_Exception() {
        var exception = Assert.Throws<ArgumentNullException>(() => new JwtAuthenticator(null));

        Assert.Equal("accessToken", exception.ParamName);
    }
}