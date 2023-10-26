import http from 'k6/http';
import {check} from 'k6';

export let options = {
    stages: [
        {duration: "1s", target: 5},
        {duration: "1m", target: 15},
        {duration: "20s", target: 0},
    ],
};

const baseUrl = 'https://localhost:7298/api';

export function setup() {
    const loginRes = http.post(`${baseUrl}/auth/token`, {
        Username: 'Petter',
        Password: 'petter1234',
    });


    const authToken = loginRes.json('token');

    check(authToken, {'logged in successfully': () => authToken !== ''});


    return authToken;

}

export default function (authToken) {

    let headers = {'Content-Type': 'application/json', 'Authorization': `Bearer ${authToken}`};

    let response = http.get(`${baseUrl}/members`, {headers: headers});

    check(response, {
        'status is 200': (r) => r.status === 200,
    });

    if (response.status !== 200) {
        console.log(response.body);
    }
}