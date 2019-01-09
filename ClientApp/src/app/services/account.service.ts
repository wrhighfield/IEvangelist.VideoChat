import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

export interface Profile {
    name: string;
    email: string;
}

export interface Authentication {
    isAuthenticated: boolean;
}

export interface Provider  {
    name: string;
}
export type Providers = Provider[];

@Injectable()
export class AccountService {
    private readonly httpOptions;

    constructor(private readonly http: HttpClient) {
        this.httpOptions = {
            headers: new HttpHeaders({
                'Access-Control-Allow-Origin': '*'
            })
        };
    }

    signOut() {
        return this.http
                   .get<any>('api/account/signOut')
                   .toPromise();
    }

    signin(provider: Provider) {
        return this.http
                   .get<any>(`/api/account/signin/${provider.name}`, this.httpOptions)
                   .toPromise();
    }

    getProviders() {
        return this.http
                   .get<Providers>('api/account/providers')
                   .toPromise();
    }

    isAuthenticated() {
        return this.http
                   .get<Authentication>('api/account/isAuthenticated')
                   .toPromise();
    }

    getProfile() {
        return this.http
                   .get<Profile>('api/account/profile')
                   .toPromise();
    }
}