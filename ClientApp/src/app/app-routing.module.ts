import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { AuthGuardService } from './services/auth-guard.service';

export const routes: Routes = [
    { path: '', redirectTo: '/home', pathMatch: 'full', canActivate: [AuthGuardService] },
    { path: 'login', component: LoginComponent } as any,
    { path: 'home', component: HomeComponent, canActivate: [AuthGuardService] },
    { path: '**', redirectTo: '' }
];

@NgModule({
    imports: [
        RouterModule.forRoot(routes)
    ],
    providers: [
        AuthGuardService
    ],
    exports: [
        RouterModule
    ]
})
export class AppRoutingModule { }