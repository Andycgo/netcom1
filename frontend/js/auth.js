const API_USUARIO = "https://localhost:44301/api/usuario";
const API_VENTA = "https://localhost:44301/api/venta";
async function registrarUsuario(event) {
  event.preventDefault();

  const usuario = {
    Nombre: document.getElementById("nombre").value.trim(),
    Apellido: document.getElementById("apellido").value.trim(),
    Correo: document.getElementById("correo").value.trim(),
    Contrasena: document.getElementById("contrasena").value.trim(),
    Telefono: document.getElementById("telefono").value.trim(),
    Direccion: document.getElementById("direccion").value.trim()
  };

  try {
    const respuesta = await fetch(API_USUARIO, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(usuario)
    });

    if (respuesta.ok) {
      alert("Usuario registrado correctamente");
      window.location.href = "login.html";
    } else {
      alert("No se pudo registrar el usuario");
    }
  } catch (error) {
    console.error(error);
    alert("Error de conexión con la API");
  }
}

async function loginUsuario(event) {
  event.preventDefault();

  const usuario = {
    Correo: document.getElementById("loginCorreo").value.trim(),
    Contrasena: document.getElementById("loginContrasena").value.trim()
  };

  try {
    const respuesta = await fetch(`${API_USUARIO}/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(usuario)
    });

    if (respuesta.ok) {
      const datosUsuario = await respuesta.json();

      localStorage.setItem("usuarioNetcom", JSON.stringify(datosUsuario));

      if (datosUsuario.Rol === "admin" || datosUsuario.rol === "admin") {
        window.location.href = "admin.html";
      } else {
        window.location.href = "index.html";
      }
    } else {
      alert("Correo o contraseña incorrectos");
    }
  } catch (error) {
    console.error(error);
    alert("Error de conexión con la API");
  }
}

function cerrarSesion() {
  localStorage.removeItem("usuarioNetcom");
  window.location.href = "login.html";
}