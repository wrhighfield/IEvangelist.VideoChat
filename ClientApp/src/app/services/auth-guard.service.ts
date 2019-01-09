import { Injectable } from '@angular/core';
import { CanActivate, Router  } from '@angular/router';
import { AccountService } from '../services/account.service';

@Injectable()
export class AuthGuardService implements CanActivate {
    constructor(
        private readonly accountService: AccountService,
        private readonly router: Router) { }

    async canActivate() {
        try {
            const result = await this.accountService.isAuthenticated();
            if (!result.isAuthenticated) {
                this.router.navigate(['/login']);
                return false;
            }
        } catch (e) {
            console.error(e);
            return false;
        }

        return true;
    }
}