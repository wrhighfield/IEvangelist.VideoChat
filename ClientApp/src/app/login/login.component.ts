import { Component, OnInit } from '@angular/core';
import { Provider, Providers, AccountService } from '../services/account.service';

@Component({
    selector: 'app-login',
    styleUrls: ['./login.component.css'],
    templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit {
    providers: Providers;

    get copyrightYear() {
        return new Date().getFullYear();
    }

    constructor(
        private readonly accountService: AccountService) { }

    async ngOnInit() {
        this.providers =
            await this.accountService.getProviders();
    }

    async onSignin(provider: Provider) {
        await this.accountService.signin(provider);
    }

    toIcon(name: string) {
        return `fa-${name.toLowerCase()}`;
    }
}