# Account management

For the web application it is possible to manage users

# When first running the Application

When using the Desktop application it is not needed to create a user. 

When first running the application, your email and password will be used to create an account. 
This account will be an admin account.

# Create a new account

The account register page is public visible when there are no users in the database.

When there are users in the database, the account register page is not visible.

At the moment email addresses are not checked for validity. In future versions this will be added.

![Account register](../assets/account_register_v050.jpg)

_Screenshot from: https://demostarsky.azurewebsites.net/starsky/account/register_

When the account is created, the user will be redirected to the login page.

![Account login](../assets/account_login_v050.jpg)

_Screenshot from: https://demostarsky.azurewebsites.net/starsky/account/login_

## Account management via the CLI

Via the `starskyadmincli` it is possible to remove accounts.

```bash
starskyadmincli
```

If you start the commandline tools without any arguments, you will see the following options:

- What is the username/email?

enter the email address of your account
If you account already exists you will see the following options:
- to toggle rights between user and admin
- to remove your account

If your account does not exist, you will be asked to create an account. 
The same password policy applies as for the web application.