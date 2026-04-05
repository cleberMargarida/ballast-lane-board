document.addEventListener('DOMContentLoaded', function () {
  var nav = document.createElement('nav');
  nav.className = 'blb-navbar';
  nav.innerHTML =
    '<div class="blb-navbar-inner">' +
      '<a href="/" class="blb-navbar-brand">' +
        '<svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" ' +
             'stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' +
          '<rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>' +
          '<rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/>' +
        '</svg>' +
        ' Ballast Lane Board' +
      '</a>' +
      '<div class="blb-navbar-actions">' +
        '<a href="/signup" class="blb-navbar-signup">Sign Up</a>' +
        '<span class="blb-navbar-signin">Sign In</span>' +
      '</div>' +
    '</div>';
  document.body.insertBefore(nav, document.body.firstChild);
});
