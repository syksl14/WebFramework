# SYuksel WebFramework
The smallest web framework to develop ASP.NET Web applications faster.
You can use the Nuget package manager to install it compiled into your project.
```
Install-Package SYuksel.WebFramework -Version 2.8.0
```

Nuget: https://www.nuget.org/packages/SYuksel.WebFramework

## Features
- Model Layer Database Management

- Objects with ASP.NET Tag library will make things easier.

- 4 database support (MSSQL, MySQL, Access, Oracle)

- Web API, Web Service and working with Classes

- JQuery plugin support

## Examples

#### Table Data Read

```asp.net
 <%
   List<Models.Views.Users_V> users = SYuksel.WebFramework.GetData<Models.Views.Users_V>("SELECT * FROM Users_V ORDER BY pkID DESC").ToList();
   foreach (Models.Views.Users_V user in users)
     {
       %>
         <tr data-id="<%= user.pkID %>">
           <td style="text-align: center;">
             <input type="checkbox" class="userchkbox" />
           </td>
           <td><%= user.UserName %></td>
           <td><%= user.Email %></td>
           <td><%= user.CrudDate %></td>
           <td><%= user.Name + " " + user.Surname %></td>
           <td><%= user.Accessed == 1 ? "Yes" : "No" %></td>
           <td>
             <div style="width: 100px;">
                <button type="button" onclick="users.edit(this);">Edit</button>
                <button type="button" onclick="users.delete(this);">Delete</button>
             </div>
           </td>
        </tr>
  <% } %>
```

#### Table Data Insert

```c#
 Model.Users user = new Model.Users();
 user.UserName = username;
 user.Password = password; 
 user.Email = email;
 user.Password = password; 
 Result r = WebFramework.InsertData(user);
 return r.Message;
```

#### SQL Execute

```c#
 String sql = "DELETE FROM Users WHERE pkID= @pkID";
 Result r = WebFramework.ExecuteNonQuery(sql, 6);
```


## Links
See here for more:

http://framework.selahattinyuksel.net

Examples: http://framework.selahattinyuksel.net/Pages/Examples/Index

Test: http://framework.selahattinyuksel.net/Pages/Demo/Index

Release Notes: http://framework.selahattinyuksel.net/Pages/ReleaseNotes/Index

Documentation: http://framework.selahattinyuksel.net/Pages/Documentation/Index

----------------------------------------------------------------------------------------------

# SYuksel WebFramework
ASP.NET Web uygulamalarını daha hızlı geliştirmek için en küçük web frameworkdür.
## Özellikler

- Model Katmanlı Veritabanı Yönetimi

- ASP.NET Tag kütüphanesi ile işleri kolaylaştıracak nesneler.

- 4 veritabanı desteği (MSSQL, MySQL, Access, Oracle)

- Web API, Web Service ve Sınıflar ile çalışma

- jQuery plugin desteği

## Bağlantılar
Daha fazlası için buraya bakın: 

http://framework.selahattinyuksel.net

Örnek Projeler: http://framework.selahattinyuksel.net/Pages/Examples/Index

Test: http://framework.selahattinyuksel.net/Pages/Demo/Index

Sürüm Notları: http://framework.selahattinyuksel.net/Pages/ReleaseNotes/Index

Dökümantasyon: http://framework.selahattinyuksel.net/Pages/Documentation/Index




