(function () {
    var widget = document.getElementById('otpWidget');
    if (!widget) {
        // Authenticated visitors don't get the widget rendered at all — nothing to wire up.
        return;
    }

    var bookingForm = document.getElementById('bookingForm');
    var emailInput = document.getElementById('BookingForm_CustomerEmail');
    var verifiedTokenInput = document.getElementById('verifiedEmailToken');
    var requestBtn = document.getElementById('otpRequestBtn');
    var confirmBtn = document.getElementById('otpConfirmBtn');
    var resendBtn = document.getElementById('otpResendBtn');
    var codeInput = document.getElementById('otpCodeInput');
    var errorEl = document.getElementById('otpError');
    var submitBtn = document.getElementById('serviceSubmitButton');

    var sessionToken = null;

    function getAntiforgeryToken() {
        var input = bookingForm ? bookingForm.querySelector('input[name="__RequestVerificationToken"]') : null;
        return input ? input.value : '';
    }

    function setState(state) {
        widget.dataset.state = state;
        if (submitBtn) {
            submitBtn.disabled = state !== 'verified';
        }
    }

    function setError(message) {
        errorEl.textContent = message || '';
    }

    function postJson(url, body) {
        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiforgeryToken()
            },
            body: JSON.stringify(body)
        }).then(function (response) {
            return response.json();
        });
    }

    function requestCode() {
        var email = emailInput ? emailInput.value.trim() : '';
        if (!email) {
            setError('Ange en e-postadress innan du begär en kod.');
            return;
        }

        setError('');
        requestBtn.disabled = true;
        resendBtn.disabled = true;

        postJson('/Service/RequestVerificationCode', { email: email })
            .then(function (data) {
                if (data.ok) {
                    sessionToken = data.sessionToken;
                    codeInput.value = '';
                    setState('pending');
                } else {
                    setError(data.error || 'Det gick inte att skicka koden.');
                }
            })
            .catch(function () {
                setError('Det gick inte att kontakta verifieringstjänsten.');
            })
            .finally(function () {
                requestBtn.disabled = false;
                resendBtn.disabled = false;
            });
    }

    function confirmCode() {
        var code = codeInput.value.trim();
        if (!code) {
            setError('Ange koden du fick via e-post.');
            return;
        }
        if (!sessionToken) {
            setError('Sessionen har gått ut. Begär en ny kod.');
            setState('idle');
            return;
        }

        setError('');
        confirmBtn.disabled = true;

        postJson('/Service/ConfirmVerificationCode', { sessionToken: sessionToken, code: code })
            .then(function (data) {
                if (data.ok) {
                    verifiedTokenInput.value = data.verifiedEmailToken;
                    setState('verified');
                } else {
                    setError(data.error || 'Fel kod. Försök igen.');
                }
            })
            .catch(function () {
                setError('Det gick inte att kontakta verifieringstjänsten.');
            })
            .finally(function () {
                confirmBtn.disabled = false;
            });
    }

    function resetVerification() {
        sessionToken = null;
        verifiedTokenInput.value = '';
        setError('');
        setState('idle');
    }

    requestBtn.addEventListener('click', requestCode);
    resendBtn.addEventListener('click', requestCode);
    confirmBtn.addEventListener('click', confirmCode);

    if (emailInput) {
        emailInput.addEventListener('input', function () {
            if (widget.dataset.state !== 'idle') {
                resetVerification();
            }
        });
    }

    setState('idle');
})();
