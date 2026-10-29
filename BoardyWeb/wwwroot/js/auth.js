// Mostrar / ocultar contraseña en los formularios de autenticación.
document.addEventListener('click', function (e) {
    const boton = e.target.closest('[data-alternar-clave]');
    if (!boton) {
        return;
    }

    const campo = boton.parentElement.querySelector('input');
    const visible = campo.type === 'text';
    campo.type = visible ? 'password' : 'text';
    boton.querySelector('i').className = visible ? 'bi bi-eye' : 'bi bi-eye-slash';
});
